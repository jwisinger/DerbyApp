using Newtonsoft.Json;

namespace ClippySharp.Models
{
    public class AgentFrameModel
    {
        [JsonProperty("duration")]
        public required int Duration { get; set; }

        [JsonProperty("images")]
        public required int[][] Images { get; set; }

        [JsonProperty("sound")]
        public required string Sound { get; set; }

        [JsonProperty("exitBranch")]
        public required string ExitBranch { get; set; }

        [JsonProperty("branching")]
        public required Dictionary<string, AgentFrameBranchModel[]> Branching { get; set; }
    }

    public class AgentFrameBranchModel
    {
        [JsonProperty("frameIndex")]
        public required int FrameIndex { get; set; }

        [JsonProperty("weight")]
        public required int Weight { get; set; }
    }
}