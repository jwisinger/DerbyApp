using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DerbyApp.Helpers
{
    internal class TrackController
    {
        public readonly string[] TrackStates = ["red", "yellow", "green", "go"];
        public bool TrackConnected = false;
        public int TrackStateNumber = 0;

        private const string _trackIp = "http://192.168.0.1/";
        private readonly DispatcherTimer _raceTimer;
        private int _raceCountDownTime = 0;
        private int _maxRaceTime = 10;
        private bool _manualControlEnabled = false;

        public event EventHandler<int> TrackStateUpdated;
        public event EventHandler<float[]> TrackTimesUpdated;
        public event EventHandler<bool> TrackStatusUpdated;

        public int MaxRaceTime
        {
            get => _maxRaceTime;
            set
            {
                _maxRaceTime = value;
                DatabaseRegistry.StoreDatabaseRegistry(null, null, null, null, value, null, null, null, null);
            }
        }

        public bool ManualControlEnabled
        {
            get => _manualControlEnabled;
            set
            {
                if (value != _manualControlEnabled)
                {
                    _manualControlEnabled = value;
                    if (_manualControlEnabled)
                    {
                        TrackStateNumber = 0;
                        _raceCountDownTime = _maxRaceTime;
                        _ = TrackMessage(TrackStates[TrackStateNumber++]);
                        DispatcherTimer t = new() { Interval = TimeSpan.FromMilliseconds(250) };
                        t.Tick += CheckSwitch;
                        t.Start();
                    }
                    else _ = TrackMessage("cancel");
                }
            }
        }

        public TrackController()
        {
            _raceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _raceTimer.Tick += TimeTickRace;
            _ = Task.Delay(1000).ContinueWith(t => CheckStatus());
        }

        public async Task CheckStatus()
        {
            try
            {
                _ = Task.Delay(5000).ContinueWith(t => CheckStatus());
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(4);
                string response = await client.GetStringAsync(new Uri("http://192.168.0.1/ping"));
                TrackStatusUpdated?.Invoke(this, true);
            }
            catch
            {
                TrackStatusUpdated?.Invoke(this, false);
            }
        }

        public async Task StartHeat()
        {
            TrackStateNumber = 0;
            await TrackMessage(TrackStates[TrackStateNumber++]);
            _raceCountDownTime = _maxRaceTime;
            _raceTimer.Start();
        }

        private async Task<string> TrackMessage(string step)
        {
            if (TrackConnected)
            {
                try
                {
                    await Task.Delay(200);
                    using HttpClient client = new();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    string response = await client.GetStringAsync(new Uri(_trackIp + step));
                    return response;
                }
                catch (HttpRequestException e)
                {
                    ErrorLogger.LogError("TrackController.TrackMessage", e);
                    MessageBox.Show(e.Message, "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }

        private void TimeTickRace(object sender, EventArgs e)
        {
            TrackStateUpdated?.Invoke(this, _raceCountDownTime);
            if (TrackStateNumber < TrackStates.Length) _ = TrackMessage(TrackStates[TrackStateNumber++]);
            else
            {
                if (_raceCountDownTime == 0)
                {
                    _raceTimer.Stop();
                    _ = GetTimes();
                }
                else _raceCountDownTime--;
            }
        }

        private async void CheckSwitch(object sender, EventArgs e)
        {
            if (ManualControlEnabled)
            {
                string response = await TrackMessage("switch");
                if (response != null)
                {
                    string[] responseArray = response.Replace("Switch ", "").Replace("\r\n", "").Split(",");
                    if (responseArray.Length == 3)
                    {
                        if ((TrackStateNumber == 1) && (responseArray[2] == "0"))
                        {
                            TimeTickRace(null, null);
                        }
                        else if ((TrackStateNumber == 2) && (responseArray[1] == "0"))
                        {
                            TimeTickRace(null, null);
                        }
                        else if ((TrackStateNumber == 3) && (responseArray[0] == "0"))
                        {
                            (sender as DispatcherTimer).Stop();
                            TimeTickRace(null, null);
                            _raceTimer.Start();
                        }
                    }
                }
            }
            else
            {
                (sender as DispatcherTimer).Stop();
            }
        }

        public async Task GetTimes()
        {
            int success = 0;
            float[] result = new float[4];
            if (TrackConnected)
            {
                try
                {
                    using HttpClient client2 = new();
                    client2.Timeout = TimeSpan.FromSeconds(5);
                    string reponse = await client2.GetStringAsync(new Uri(_trackIp + "read"));
                    if (reponse.Contains("Times"))
                    {
                        string[] times = reponse.Split(' ');
                        if (times.Length == 2)
                        {
                            times = times[1].Split(',');
                            if (times.Length == 4)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    if (float.TryParse(times[i], out result[i]))
                                    {
                                        if (result[i] < 0.1) result[i] = 10.0F;
                                        success++;
                                    }
                                }
                            }
                        }
                    }
                    if (success == 4) TrackTimesUpdated?.Invoke(this, result);
                    else MessageBox.Show("Received a bad response from track.", "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (HttpRequestException e)
                {
                    ErrorLogger.LogError("TrackController.GetTimes", e);
                    MessageBox.Show(e.Message, "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
