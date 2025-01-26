using ClippySharp.Models;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Threading;

namespace ClippySharp
{

    public class Agent
    {
        public event EventHandler? NeedsRender;

        internal AgentAnimator? Animator { get; }
        internal AgentModel? Model { get; }
        public Size? ImageSize { get; }
        public bool Sound { get; set; } = true;
        private readonly DispatcherTimer _idleTimer;
        private readonly DispatcherTimer _timeoutTimer;

        public Agent(string agent)
        {
            _idleTimer = new DispatcherTimer();
            _timeoutTimer = new DispatcherTimer();
            string agentJson = AssemblyHelper.ReadResourceString(agent, "agent.json");
            Model = JsonConvert.DeserializeObject<AgentModel>(agentJson);
            if (Model != null)
            {
                ImageSize = new Size(Model.FrameSize[0], Model.FrameSize[1]);
                Animator = new AgentAnimator(agent, this);
                Animator.NeedsRefresh += Animator_NeedsRefresh;
                Animator.AnimationEnded += Animator_AnimationEnded;
                _idleTimer.Interval = TimeSpan.FromSeconds(5);
                _idleTimer.Tick += IdleTimer_Tick;
                _idleTimer.Start();
                _timeoutTimer.Interval = TimeSpan.FromSeconds(5);
                _timeoutTimer.Tick += TimeoutTimer_Tick;
            }
        }

        void IdleTimer_Tick(object? sender, EventArgs e)
        {
            AgentAnimation? animation = Animator?.GetIdleAnimation();
            if (animation != null) Animator?.ShowAnimation(animation);

        }

        void TimeoutTimer_Tick(object? sender, EventArgs e)
        {
            //MessageBox.Show("Timeout on animation");
            _timeoutTimer.Stop();
            _idleTimer.Start();
        }

        public void PlaySound(string? id)
        {
            if (Sound && !string.IsNullOrEmpty(id))
            {
                var sound = Animator?.Sounds?.FirstOrDefault(s => s.Id == id);
                if (sound != null && AgentEnvironment.Current.SoundPlayer != null)
                {
                    AgentEnvironment.Current.SoundPlayer.Play(sound);
                }
            }
        }

        public bool Play(string animation)
        {
            if (Animator == null || !Animator.HasAnimation(animation)) return false;
            _idleTimer.Stop();
            _timeoutTimer.Start();
            Animator?.ShowAnimation(animation);
            return true;
        }

        public void Stop()
        {
            Animator?.ExitAnimation();
        }

        void Animator_NeedsRefresh(object? sender, EventArgs e)
        {
            NeedsRender?.Invoke(this, e);
        }

        void Animator_AnimationEnded(object? sender, EventArgs e)
        {
            _timeoutTimer.Stop();
            _idleTimer.Start();
        }

        public ImageSource? GetCurrentImage()
        {
            return Animator?.GetCurrentImage();
        }

        public void Dispose()
        {
            if (Animator != null) Animator.NeedsRefresh -= Animator_NeedsRefresh;
        }
    }
}
