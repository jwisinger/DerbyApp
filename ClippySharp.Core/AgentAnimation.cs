using ClippySharp.Models;

namespace ClippySharp
{
    internal class AgentAnimation
    {
        const string Idle = "Idle";

        public string Name { get; }

        public List<AgentAnimationFrame> Frames { get; }

        readonly AgentAnimationModel model;
        internal bool UseExitBranching => model.UseExitBranching;

        public AgentAnimation(AgentAnimator animator, string name, AgentAnimationModel model)
        {
            this.model = model;

            Name = name;
            Frames = [];

            foreach (var frame in model.Frames)
            {
                Frames.Add(new AgentAnimationFrame(animator, frame));
            }
        }

        internal bool IsIdle()
        {
            return Name.StartsWith(Idle, StringComparison.Ordinal);
        }
    }
}
