using ClippySharp.Models;
using System.Drawing;
using System.Windows.Media;

namespace ClippySharp
{
    internal class AgentAnimationFrame(AgentAnimator animator, AgentFrameModel mode)
    {
        readonly AgentAnimator animator = animator;
        readonly AgentFrameModel model = mode;

        public string ExitBranch => model.ExitBranch;

        public Dictionary<string, AgentFrameBranchModel[]> Branching => model.Branching;

        public int Duration => model.Duration;
        public string Sound => model.Sound;

        public ImageSource? GetImage()
        {
            if (model.Images != null)
            {
                return animator.GetImage(model.Images);
            }
            return null;
        }
    }
}
