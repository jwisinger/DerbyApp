using ClippySharp.Models;
using Newtonsoft.Json;
using System.Windows.Media;

namespace ClippySharp
{

    public class Agent
    {
        public event EventHandler? NeedsRender;

        public bool Sound { get; set; } = true;
        public Size ImageSize { get; }

        internal AgentAnimator? Animator { get; }
        internal AgentModel? Model { get; }

        readonly QueueProcessor queue;

        public Agent(string agent)
        {
            //we initialize context
            queue = new QueueProcessor();

            var agentJson = AssemblyHelper.ReadResourceString(agent, "agent.json");
            Model = JsonConvert.DeserializeObject<AgentModel>(agentJson);
            if (Model == null) return;

            ImageSize = new Size(Model.FrameSize[0], Model.FrameSize[1]);
            Animator = new AgentAnimator(agent, this);

            Animator.NeedsRefresh += Animator_NeedsRefresh;
        }

        public void PlaySound(string? id)
        {
            if (!Sound || string.IsNullOrEmpty(id))
            {
                return;
            }
            var sound = Animator?.Sounds?.FirstOrDefault(s => s.Id == id);
            if (sound != null && AgentEnvironment.Current.SoundPlayer != null)
            {
                AgentEnvironment.Current.SoundPlayer.Play(sound);
            }
        }

        public void Stop()
        {
            this.queue.Clear();
            Animator?.ExitAnimation();
        }

        public bool Play(string animation, int timeout = 5000)
        {
            if (!Animator.HasAnimation(animation)) return false;

            AddToQueue(() =>
            {
                void handler(object? s, AnimationStateEventArgs e)
                {
                    Animator.AnimationEnded -= handler;

                    if (timeout > 0)
                    {
                        //window.setTimeout($.proxy(function() {
                        //    if (completed) return;
                        //    // exit after timeout
                        //    this._animator.exitAnimation();
                        //}, this), timeout)
                    }
                }

                Animator.AnimationEnded += handler;
                Animator?.ShowAnimation(animation);
            });
            return true;
        }

        void Animator_NeedsRefresh(object? sender, EventArgs e)
        {
            NeedsRender?.Invoke(this, e);
        }

        public ImageSource? GetCurrentImage()
        {
            return Animator?.GetCurrentImage();
        }

        void AddToQueue(Action p)
        {
            queue.Enqueue(p);
        }

        public void Dispose()
        {
            if (Animator != null) Animator.NeedsRefresh -= Animator_NeedsRefresh;
        }
    }
}
