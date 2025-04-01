using DerbyApp.Helpers;
using DerbyApp.RaceStats;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DerbyApp.Assistant
{
    public class Announcer
    {
        public bool Muted = false;
        public VoiceInterface Voice = new();
        public string CurrentVoiceName = "HFC Female";

        private readonly Dictionary<string, string> _voices = new()
        {
            { "Amy", "en_US-amy-medium" },
            { "Arctic", "en_US-arctic-medium"},
            { "Bryce", "en_US-bryce-medium"},
            { "Danny", "en_US-danny-low"},
            { "Joe", "en_US-joe-medium"},
            { "John", "en_US-john-medium"},
            { "Kathleen", "en_US-kathleen-low"},
            { "Kristin", "en_US-kristin-medium"},
            { "Kusal", "en_US-kusal-medium"},
            { "Norman", "en_US-norman-medium"},
            { "Ryan", "en_US-ryan-high"},
            { "HFC Female", "en_US-hfc_female-medium"},
            { "HFC Male", "en_US-hfc_male-medium"},
            { "L2 Arctic", "en_US-l2arctic-medium"},
            { "Lessac", "en_US-lessac-high"},
            { "LibriTTS", "en_US-libritts-high"},
            { "LibriTTS R", "en_US-libritts_r-medium"},
            { "LJ Speech", "en_US-ljspeech-high"},
        };

        public Announcer()
        {
            _ = Init();
        }

        private async Task Init()
        {
            await Voice.Config(_voices[CurrentVoiceName]);
            await Voice.Start();
        }

        public void Speak(string s)
        {
            if (!Muted) Voice.Speak(s);
        }

        public void Silence()
        {
            _ = Voice.Cancel();
        }

        public async Task<bool> SelectVoice(string voiceName)
        {
            CurrentVoiceName = voiceName;
            await Voice.Config(_voices[CurrentVoiceName]);
            _ = Voice.Start();
            Speak("The announcer is now " + CurrentVoiceName + ".");
            return true;
        }

        public List<string> GetVoiceNames()
        {
            return [.. _voices.Keys]; ;
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
                case 0:
                    Speak("Drivers ready!");
                    //Speak("Goonter!");
                    //Speak("Unos!");
                    break;
                case 1:
                    Speak("On your marks!");
                    //Speak("Glieben!");
                    //Speak("Dose!");
                    break;
                case 2:
                    Speak("Get set!");
                    //Speak("Glouken!");
                    //Speak("Trace!");
                    break;
                case 3:
                    Speak("Go!");
                    //Speak("Globen!");
                    //Speak("Catursay!");
                    break;
            }
        }
    }
}
