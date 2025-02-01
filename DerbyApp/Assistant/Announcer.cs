using System.Collections.ObjectModel;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace DerbyApp.Assistant
{
    public class Announcer
    {
        public bool Muted = false;
        public SpeechSynthesizer Synth = new();
        public VoiceInterface Voice = new();

        public Announcer()
        {
            _ = Voice.Run();
        }

        private void Speak(string s)
        {
            if (!Muted) Voice.Speak(s);
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
