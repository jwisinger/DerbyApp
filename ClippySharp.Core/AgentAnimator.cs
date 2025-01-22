using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace ClippySharp.Core
{

	internal class AgentAnimator
    {
		static readonly Random rnd = new Random (DateTime.Now.Millisecond);
		float GetRandom () => rnd.Next (0, 1);

		public event EventHandler<AnimationStateEventArgs> AnimationEnded;
        public event EventHandler NeedsRefresh;

		public List<SoundData> Sounds { get; }
		public List<AgentAnimation> Animations { get; }

		internal string currentAnimationName;

        readonly IBitmapWrapper bitmapSheet;
        readonly System.Timers.Timer aTimer;
		readonly Agent agent;

		bool _exiting;
		internal bool _started;
		int currentFrameIndex;

		AgentAnimation currentAnimation;
		AgentAnimationFrame currentFrame;

        public AgentAnimator(string name, Agent agent)
        {
            this.agent = agent;
            //sound processing
            var soundJson = AssemblyHelper.ReadResourceString(name, "sounds-mp3.json");
            var soundData = JsonConvert.DeserializeObject<Dictionary<string, string>>(soundJson);

            Sounds = new List<SoundData>();
            foreach (var data in soundData)
            {
                Sounds.Add(new SoundData(data.Key, data.Value));
            }

            //image processing
            bitmapSheet = AgentEnvironment.Current.Delegate.GetImageSheet(name, "map.png");

            Animations = new List<AgentAnimation>();
            foreach (var animationKey in agent.Model.Animations)
            {
                //if (animationKey.Value.TryGetValue("frames", out AgentAnimationModel animation))
                //{
                    Animations.Add(new AgentAnimation(this, animationKey.Key, animationKey.Value));
               // };
            }

            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += ATimer_Elapsed;
        }

        void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            aTimer.Stop();
            Step();
        }

		#region Public API

		public IImageWrapper GetImage (int[][] image)
		{
			return AgentEnvironment.Current.Delegate.GetImage (bitmapSheet, image, agent);
		}

		public bool HasAnimation (string name)
		{
			return Animations.Any (s => s.Name == name);
		}

		public bool IsIdleAnimation ()
		{
			return currentAnimation?.IsIdle () ?? false;
		}

		public bool ShowAnimation (string animationName)
		{
			this._exiting = false;
			if (!this.HasAnimation (animationName)) {
				return false;
			}

			this.currentAnimation = Animations.FirstOrDefault (s => s.Name == animationName);
			this.currentAnimationName = animationName;

            this.currentFrame = null;
            this.currentFrameIndex = 0;

            if (!this._started) {
				this.Step ();
				this._started = true;
			}

			return true;
		}

		public static AgentAnimation GetRandomAnimation (List<AgentAnimation> animations)
		{
			return animations[(int)rnd.Next (0, animations.Count - 1)];
		}

		public AgentAnimation GetRandomAnimation ()
		{
			return GetRandomAnimation (Animations);
		}

		public AgentAnimation GetIdleAnimation ()
		{
			var r = new List<AgentAnimation> ();
			foreach (var animation in Animations) {
				if (animation.IsIdle ()) {
					r.Add (animation);
				}
			}

			return GetRandomAnimation (r);
		}

		public void OnQueueEmpty ()
		{
			if (agent.Hidden || this.IsIdleAnimation ()) return;
			var idleAnim = this.GetIdleAnimation ();
			this.ShowAnimation (idleAnim.Name);
		}

		public IImageWrapper GetCurrentImage ()
		{
			return currentFrame?.GetImage ();
		}

		public void ExitAnimation ()
		{
			_exiting = true;
		}

        public bool IsAtLastFrame ()
        {
            return this.currentFrameIndex >= this.currentAnimation.Frames.Count - 1;
        }

        public void Step ()
        {
			//this is under a timer an needs

			AgentAnimation animation = this.currentAnimation;
			var frame = this.currentFrame;

			if (animation == null) return;

            var newFrameIndex = Math.Min(this.GetNextAnimationFrame(), animation.Frames.Count - 1);
            var frameChanged = frame != null && this.currentFrameIndex != newFrameIndex;
            this.currentFrameIndex = newFrameIndex;

            // always switch frame data, unless we're at the last frame of an animation with a useExitBranching flag.
            if (!(this.IsAtLastFrame() && animation.UseExitBranching))
            {
				currentFrame = frame = animation.Frames[this.currentFrameIndex];
            }

            NeedsRefresh?.Invoke(this, EventArgs.Empty);
            if (frameChanged && this.IsAtLastFrame())
            {
				_started = false;
				if (animation.UseExitBranching && !this._exiting)
                {
                    AnimationEnded?.Invoke(this, new AnimationStateEventArgs ( currentAnimationName, AnimationStates.Waiting));
                }
                else
                {
                    AnimationEnded?.Invoke(this, new AnimationStateEventArgs(currentAnimationName, AnimationStates.Exited));
                }
            }
            else
            {
                agent.PlaySound(currentFrame?.Sound);
                if (frame.Duration > 0) aTimer.Interval = frame.Duration;
                else aTimer.Interval = 10;
                aTimer.Start();
            }
        }

        public int GetNextAnimationFrame ()
        {
            if (currentFrame == null) 
                return 0;
            if (currentFrameIndex >= currentAnimation.Frames.Count)
                return 0;

            var branching = currentFrame.Branching;

            if (this._exiting && currentFrame.ExitBranch != null) 
            {
                return int.Parse (currentFrame.ExitBranch);
            }
            
            if (branching != null)
            {
                var random = GetRandom () * 100;
                var branches = branching["branches"];

                for (var i = 0; i < branches.Count (); i++)
                {
                    var branch = branches[i];
                    if (random <= branch.Weight)
                    {
                        return branch.FrameIndex;
                    }

                    random -= branch.Weight;
                }
            }

            return this.currentFrameIndex + 1;
        }

		public void Pause()
        {
            aTimer.Stop();
        }

        public void Resume()
        {
            aTimer.Start();
        }

		#endregion

	}

	public class AnimationStateEventArgs : EventArgs
    {
        public AnimationStateEventArgs(string name, AnimationStates states)
        {
            Name = name;
            State = states;
        }

        public string Name { get; }
        public AnimationStates State { get; }
    }
}
