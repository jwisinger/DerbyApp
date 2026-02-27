using NAudio.Wave;
using PiperSharp;
using PiperSharp.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DerbyApp.Assistant
{
    public class VoiceInterface
    {
        PiperWaveProvider _provider;
        CancellationTokenSource cancellationTokenSource = new();

        public async Task Config(string ModelKey)
        {
            VoiceModel model;

            if (!File.Exists(PiperDownloader.DefaultPiperExecutableLocation))
            {
                await PiperDownloader.DownloadPiper().ExtractPiper(PiperDownloader.DefaultLocation);
            }
            var modelPath = Path.Join(PiperDownloader.DefaultModelLocation, ModelKey);

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
        }

        public async Task Start()
        {
            var playbackThread = new Thread(PlaybackThread);
            _provider?.Start();
            cancellationTokenSource = new();
            playbackThread.Start(cancellationTokenSource.Token);
            await _provider?.WaitForExit(cancellationTokenSource.Token);
        }

        public async Task Restart()
        {
            await cancellationTokenSource.CancelAsync();
            await Start();
        }

        public async Task Cancel()
        {
            await cancellationTokenSource.CancelAsync();
        }

        public void PlaybackThread(object obj)
        {
            CancellationToken token = (CancellationToken)obj;

            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(_provider);
            outputDevice.Play();
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                if (token.IsCancellationRequested)
                {
                    outputDevice.Stop();
                }
                Thread.Sleep(500);
            }
        }

        public void Speak(string text)
        {
            _provider?.InferPlayback(text).GetAwaiter().GetResult();
        }
    }
}
