using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using PiperSharp;
using PiperSharp.Models;

namespace DerbyApp.Assistant
{
    public class VoiceInterface
    {
        PiperWaveProvider _provider;

        public async Task Run()
        {
#warning TODO: Make this voice configurable
            const string ModelKey = "en_US-amy-medium";
            if (!File.Exists(PiperDownloader.DefaultPiperExecutableLocation))
            {
                await PiperDownloader.DownloadPiper().ExtractPiper(PiperDownloader.DefaultLocation);
            }
            var modelPath = Path.Join(PiperDownloader.DefaultModelLocation, ModelKey);
            VoiceModel model = null;
            if (Directory.Exists(modelPath))
            {
                model = await VoiceModel.LoadModelByKey(ModelKey);
            }
            else
            {
                model = await PiperDownloader.DownloadModelByKey(ModelKey);
            }

            _provider = new PiperWaveProvider(new PiperConfiguration()
            {
                Model = model,
                UseCuda = false
            });
            _provider.Start();

            var playbackThread = new Thread(PlaybackThread);
            playbackThread.Start();

            await _provider.WaitForExit();
        }

        public void PlaybackThread()
        {
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(_provider);
            outputDevice.Play();
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(500);
            }
        }

        public void Speak(string text)
        {
            _provider.InferPlayback(text).GetAwaiter().GetResult();
        }
    }
}
