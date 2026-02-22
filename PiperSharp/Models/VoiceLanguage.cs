using System.Text.Json.Serialization;

namespace PiperSharp.Models
{
    public class VoiceLanguage
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }

        [JsonPropertyName("family")]
        public required string Family { get; set; }

        [JsonPropertyName("name_english")]
        public required string Name { get; set; }
    }
}