using System.Collections.ObjectModel;
using System.Speech.Synthesis;

namespace DerbyApp.Assistant
{
    internal class Announcer
    {
        public bool Muted = false;
        public SpeechSynthesizer Synth = new();

        private void Speak(string s)
        {
            if (!Muted) Synth.SpeakAsync(s);
        }

        public ReadOnlyCollection<InstalledVoice> GetVoices()
        {
            return Synth.GetInstalledVoices();
        }

        public void Introduction()
        {
            string s = "The announcer is now " + Synth.Voice.Name;
            Speak(s);
        }

        public void StartRace(int step)
        {
            //Synth.SetOutputToDefaultAudioDevice();

            switch (step)
            {
                case 1:
                    Speak("On your marks!");
                    break;
                case 2:
                    Speak("Get set!");
                    break;
                case 3:
                    Speak("Go!");
                    break;
            }
        }
    }
}
