using System.Collections.Generic;
using ClippySharp.Core.Models;

namespace ClippySharp.Core
{
    internal class AgentAnimationFrame
    {
        readonly AgentAnimator animator;
        readonly AgentFrameModel model;

        public string ExitBranch => model.ExitBranch;

        public Dictionary<string, AgentFrameBranchModel[]> Branching => model.Branching;

        public int Duration => model.Duration;
        public string Sound => model.Sound;

        public AgentAnimationFrame(AgentAnimator animator, AgentFrameModel mode)
        {
            this.animator = animator;
            this.model = mode;
        }

        public IImageWrapper? GetImage ()
        {
            if (model.Images != null)
            {
                return animator.GetImage(model.Images);
            }
            return null;
        }
    }
}
