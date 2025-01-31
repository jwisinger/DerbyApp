using ClippySharp.Models;
using System.Drawing;
using System.Windows.Media;

namespace ClippySharp
{
    internal class AgentAnimationFrame(AgentFrameModel model)
    {
        readonly AgentFrameModel _model = model;

        public string ExitBranch => _model.ExitBranch;

        public Dictionary<string, AgentFrameBranchModel[]> Branching => _model.Branching;

        public int Duration => _model.Duration;
        public string Sound => _model.Sound;
        public int[][] Images => _model.Images;
    }
}
