using System.Speech.Synthesis;

namespace DerbyApp.Assitant
{
    internal class Announcer
    {
        public static void StartRace(int step)
        {
            SpeechSynthesizer synth = new();
            synth.SetOutputToDefaultAudioDevice();

            switch (step)
            {
                case 1:
                    synth.SpeakAsync("On your marks!");
                    break;
                case 2:
                    synth.SpeakAsync("Get set!");
                    break;
                case 3:
                    synth.SpeakAsync("Go!");
                    break;
            }
        }
    }
}
