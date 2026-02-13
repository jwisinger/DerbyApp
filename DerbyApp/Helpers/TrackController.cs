using DerbyApp.Assistant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        private readonly DispatcherTimer _raceTimer;
        private bool _manualControlEnabled = false;
        private int _raceCountDownTime = 0;

        public event EventHandler<int> TrackStateUpdated;

        public string TrackStateString
        {
            get
            {
                if (TrackStateNumber < TrackStates.Length) return TrackStates[TrackStateNumber];
                else return "";
            }
        }

        public TrackController()
        {
            _raceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _raceTimer.Tick += TimeTickRace;
        }

        public async Task StartHeat(int maxRaceTime)
        {
            TrackStateNumber = 0;
            await TrackMessage(TrackStates[TrackStateNumber++]);
            _raceCountDownTime = maxRaceTime;
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
                    string response = await client.GetStringAsync(new Uri("http://192.168.0.1/" + step));
                    return response;
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show(e.Message, "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }

        private void TimeTickRace(object sender, EventArgs e)
        {
            TrackStateUpdated?.Invoke(this, _raceCountDownTime);
            if (TrackStateNumber < TrackStates.Length) TrackStateNumber++;
            else
            {
                if (_raceCountDownTime == 0)
                {
                    _raceTimer.Stop();
                    //ButtonGetTimes_Click(sender, null);
                }
                _raceCountDownTime--;
            }
        }

        private async void CheckSwitch(object sender, EventArgs e)
        {
            if (_manualControlEnabled)
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
    }
}
