using Newtonsoft.Json;

namespace ClippySharp.Models
{
    public class AgentAnimationModel
    {
        [JsonProperty("useExitBranching")]
        public bool UseExitBranching { get; set; }

        [JsonProperty("frames")]
        public required AgentFrameModel[] Frames { get; set; }
    }

    public class AgentModel
    {
        [JsonProperty("overlayCount")]
        public int OverlayCount { get; set; }

        [JsonProperty("sounds")]
        public required string[] Sounds { get; set; }

        [JsonProperty("framesize")]
        public required int[] FrameSize { get; set; }

        [JsonProperty("animations")]
        public required Dictionary<string, AgentAnimationModel> Animations { get; set; }
    }
}
