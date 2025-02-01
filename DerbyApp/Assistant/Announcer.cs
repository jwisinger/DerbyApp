using DerbyApp.Helpers;
using DerbyApp.RaceStats;
using System.Collections.ObjectModel;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Documents;

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

        public void SayNames(TrulyObservableCollection<Racer> Racers)
        {
            foreach (Racer racer in Racers)
            {
                Speak("In lane " + racer.Lane + ". " + racer.RacerName + ".");
            }
        }

        public void StartRace(int step)
        {
            switch (step)
            {
                case 1:
                    Speak("On your marks!");
                    //Speak("Goonter!");
                    //Speak("Unos!");
                    //Speak("Let it go!");
                    break;
                case 2:
                    Speak("Get set!");
                    //Speak("Glieben!");
                    //Speak("Dose!");
                    //Speak("Let it go!");
                    break;
                case 3:
                    Speak("Go!");
                    //Speak("Glouken! Globen!");
                    //Speak("Trace! Catursay!");
                    //Speak("Can't hold it back anymore!");
                    break;
            }
        }
    }
}
