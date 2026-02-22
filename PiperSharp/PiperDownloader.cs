using PiperSharp.Models;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PiperSharp
{
    public static partial class PiperDownloader
    {
        private const string PIPER_REPO_URL = "https://github.com/rhasspy/piper";
        private const string MODEL_LIST_URL = "https://huggingface.co/rhasspy/piper-voices/raw/main/voices.json";
        private const string MODEL_DOWNLOAD_URL =
            "https://huggingface.co/rhasspy/piper-voices/resolve/main/MODEL_FILE_URL?download=true";

        public static string DefaultLocation => Directory.GetCurrentDirectory();
        public static string DefaultModelLocation => Path.Join(DefaultLocation, "models");
        public static string DefaultPiperLocation => Path.Join(DefaultLocation, "piper");
        public static string DefaultPiperExecutableLocation => Path.Join(DefaultPiperLocation, PiperExecutable);
        public static string PiperExecutable => Environment.OSVersion.Platform == PlatformID.Win32NT ? "piper.exe" : "piper";


        private static Dictionary<string, VoiceModel>? _voiceModels;
        private static readonly Regex RemoveLastSlash = RemoveSlash();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public static async Task<Stream> DownloadPiper(string version = "latest", string repo = PIPER_REPO_URL)
        {
            if (!Environment.Is64BitOperatingSystem)
                throw new NotSupportedException();
            var fileName = "piper_windows_amd64.zip";
            version = version == "latest" ? "latest/download" : $"download/{version}";
            var url = $"{RemoveLastSlash.Replace(repo, "")}/releases/{version}/{fileName}";
            var client = new HttpClient();
            var downloadStream = await client.GetStreamAsync(url);
            return downloadStream;
        }

        public static Task<string> ExtractPiper(this Task<Stream> downloadStream)
            => ExtractPiper(downloadStream, DefaultLocation);
        public static async Task<string> ExtractPiper(this Task<Stream> downloadStream, string extractTo)
            => ExtractPiper(await downloadStream, extractTo);
        public static string ExtractPiper(this Stream downloadStream, string extractTo)
        {
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            if (isWindows)
            {
                using var archive = new ZipArchive(downloadStream);
                archive.ExtractToDirectory(extractTo);
            }
            else
            {
                using var reader = ReaderFactory.Open(downloadStream);
                var piperPath = Path.Join(extractTo, "piper")!;
                Queue<(string from, string to)> expectedSymlinks =
                    new();
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteEntryToDirectory(extractTo, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true,
                            WriteSymbolicLink = ((to, from) =>
                            {
                                expectedSymlinks.Enqueue((from, Path.GetFileName(to)));
                            })
                        });
                    }
                }
                while (expectedSymlinks.TryDequeue(out var link))
                {
                    var fromPath = Path.Join(piperPath, link.from);
                    var toPath = Path.Join(piperPath, link.to);
                    if (File.Exists(fromPath))
                    {
                        File.Copy(fromPath, toPath);
                        continue;
                    }
                    if (File.Exists(toPath)) continue;

                    // Could not copy and file does not exist, back in queue
                    expectedSymlinks.Enqueue(link);
                }
            }
            return extractTo;
        }

        public static async Task<VoiceModel?> GetModelByKey(string modelName)
        {
            await GetHuggingFaceModelList();
            return _voiceModels?.GetValueOrDefault(modelName);
        }

        public static async Task<VoiceModel?> TryGetModelByKey(string modelKey)
        {
            await GetHuggingFaceModelList();
            return _voiceModels?.GetValueOrDefault(modelKey);
        }

        public static async Task<Dictionary<string, VoiceModel>?> GetHuggingFaceModelList()
        {
            if (_voiceModels != null) return _voiceModels;
            var client = new HttpClient();
            var response = await client.GetAsync(MODEL_LIST_URL);
            if (!response.IsSuccessStatusCode) throw new HttpRequestException();
            var data = await response.Content.ReadAsStringAsync() ?? throw new ApplicationException();
            _voiceModels = JsonSerializer.Deserialize<Dictionary<string, VoiceModel>>(data);
            return _voiceModels;
        }

        public static async Task<VoiceModel> DownloadModelByKey(string modelKey)
        {
            var model = await GetModelByKey(modelKey) ?? throw new ArgumentException($"Model {modelKey} does not exist!", nameof(modelKey));
            return await model.DownloadModel();
        }
        public static Task<VoiceModel> DownloadModel(this VoiceModel model)
            => model.DownloadModel(DefaultModelLocation);
        public static async Task<VoiceModel> DownloadModel(this VoiceModel model, string saveModelTo)
        {
            var path = Path.Join(saveModelTo, model.Key);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var client = new HttpClient();
            if (model.Files == null) return model;
            foreach (var file in model.Files.Keys)
            {
                var fileName = Path.GetFileName(file);
                var filePath = Path.Join(path, fileName);
                var downloadStream = await client.GetStreamAsync(MODEL_DOWNLOAD_URL.Replace("MODEL_FILE_URL", file));
                // Load Audio configuration from .onnx.json file
                if (fileName.EndsWith(".onnx.json"))
                {
                    var ms = new MemoryStream();
                    await downloadStream.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var modelJson = JsonSerializer.Deserialize<VoiceModel>(ms);
                    model.Audio = modelJson!.Audio;
                    ms.Seek(0, SeekOrigin.Begin);
                    await using var fs = File.OpenWrite(filePath);
                    await ms.CopyToAsync(fs);
                    fs.Close();
                }
                else
                {
                    await using var fs = File.OpenWrite(filePath);
                    await downloadStream.CopyToAsync(fs);
                    fs.Close();
                }
                downloadStream.Close();
            }

            model.ModelLocation = path;
            await using var modelInfoFile = File.OpenWrite(Path.Join(path, "model.json"));
            await JsonSerializer.SerializeAsync<VoiceModel>(modelInfoFile, model, _jsonOptions);
            modelInfoFile.Close();

            return model;
        }

        [GeneratedRegex(@"\/$", RegexOptions.Compiled)]
        private static partial Regex RemoveSlash();
    }
}