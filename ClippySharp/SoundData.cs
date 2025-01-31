
namespace ClippySharp
{
    public class SoundData
    {
        public string Id { get; }
        public SoundType SoundType { get; } = SoundType.NotDetected;
        public string Data { get; }
        public string Base { get; }

        public SoundData(string id, string data)
        {
            this.Id = id;
            var index = data.IndexOf(';');
            string type = data["data:".Length..index];
            if (type == "audio/mpeg")
            {
                SoundType = SoundType.Mpeg;
            }

            var separator = data.IndexOf(',');

            index++;
            Base = data[index..separator];
            Data = data[(separator + 1)..];
        }
    }
}
