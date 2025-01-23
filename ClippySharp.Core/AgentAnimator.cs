using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClippySharp
{

    internal class AgentAnimator
    {
        static readonly Random rnd = new(DateTime.Now.Millisecond);

        public event EventHandler<AnimationStateEventArgs>? AnimationEnded;
        public event EventHandler? NeedsRefresh;

        public List<SoundData>? Sounds { get; }
        public List<AgentAnimation> Animations { get; }

        internal string? currentAnimationName;

        readonly Bitmap bitmapSheet;
        readonly System.Timers.Timer aTimer;
        readonly Agent agent;

        bool _exiting;
        internal bool _started;
        int currentFrameIndex;

        AgentAnimation? currentAnimation;
        AgentAnimationFrame? currentFrame;

        public AgentAnimator(string name, Agent agent)
        {
            this.agent = agent;
            //sound processing
            var soundJson = AssemblyHelper.ReadResourceString(name, "sounds-mp3.json");
            var soundData = JsonConvert.DeserializeObject<Dictionary<string, string>>(soundJson);

            if (soundData != null)
            {
                Sounds = [];
                foreach (var data in soundData)
                {
                    Sounds.Add(new SoundData(data.Key, data.Value));
                }
            }

            //image processing
            bitmapSheet = GetImageSheet(name, "map.png");

            if (agent.Model != null)
            {
                Animations = [];
                foreach (var animationKey in agent.Model.Animations)
                {
                    //if (animationKey.Value.TryGetValue("frames", out AgentAnimationModel animation))
                    //{
                    Animations.Add(new AgentAnimation(this, animationKey.Key, animationKey.Value));
                    // };
                }
            }
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += ATimer_Elapsed;
        }

        public static Bitmap GetImageSheet(string agentName, string resourceName)
        {
            BitmapImage? bitmapImage = AssemblyHelper.ReadResourceImage(agentName, resourceName);

            using MemoryStream outStream = new();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            enc.Save(outStream);
            var bitmap = new Bitmap(new Bitmap(outStream));
            bitmap.MakeTransparent();
            return bitmap;
        }

        void ATimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            aTimer.Stop();
            Step();
        }

        #region Public API

        private static BitmapImage Convert(Bitmap bitmap)
        {
            using MemoryStream memory = new();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public ImageSource? GetImage(int[][] image)
        {
            ImageSource? imageView = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Bitmap img = new((int)agent.ImageSize.Width, (int)agent.ImageSize.Height);
                using (Graphics gr = Graphics.FromImage(img))
                {
                    for (int i = 0; i < image.Length; i++)
                    {
                        RectangleF cropRect = new(image[i][0], image[i][1], (int)agent.ImageSize.Width, (int)agent.ImageSize.Height);
                        gr.DrawImage(bitmapSheet, new RectangleF(0, 0, img.Width, img.Height), cropRect, GraphicsUnit.Pixel);
                    }
                }
                var imgSource = Convert(img);
                imageView = imgSource;
            });

            return imageView;
        }

        public bool HasAnimation(string name)
        {
            return Animations.Any(s => s.Name == name);
        }

        public bool IsIdleAnimation()
        {
            return currentAnimation?.IsIdle() ?? false;
        }

        public bool ShowAnimation(string animationName)
        {
            this._exiting = false;
            if (!this.HasAnimation(animationName))
            {
                return false;
            }

            currentAnimation = Animations.FirstOrDefault(s => s.Name == animationName);
            this.currentAnimationName = animationName;

            this.currentFrame = null;
            this.currentFrameIndex = 0;

            if (!this._started)
            {
                this.Step();
                this._started = true;
            }

            return true;
        }

        public static AgentAnimation GetRandomAnimation(List<AgentAnimation> animations)
        {
            return animations[(int)rnd.Next(0, animations.Count - 1)];
        }

        public AgentAnimation GetRandomAnimation()
        {
            return GetRandomAnimation(Animations);
        }

        public AgentAnimation GetIdleAnimation()
        {
            var r = new List<AgentAnimation>();
            foreach (var animation in Animations)
            {
                if (animation.IsIdle())
                {
                    r.Add(animation);
                }
            }

            return GetRandomAnimation(r);
        }

        public void OnQueueEmpty()
        {
            if (this.IsIdleAnimation()) return;
            var idleAnim = this.GetIdleAnimation();
            this.ShowAnimation(idleAnim.Name);
        }

        public ImageSource? GetCurrentImage()
        {
            return currentFrame?.GetImage();
        }

        public void ExitAnimation()
        {
            _exiting = true;
        }

        public bool IsAtLastFrame()
        {
            return this.currentFrameIndex >= this.currentAnimation?.Frames.Count - 1;
        }

        public void Step()
        {
            //this is under a timer an needs

            AgentAnimation? animation = this.currentAnimation;
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
                    AnimationEnded?.Invoke(this, new AnimationStateEventArgs(currentAnimationName, AnimationStates.Waiting));
                }
                else
                {
                    AnimationEnded?.Invoke(this, new AnimationStateEventArgs(currentAnimationName, AnimationStates.Exited));
                }
            }
            else
            {
                if (currentFrame?.Sound != null) agent.PlaySound(currentFrame?.Sound);
                if (frame != null && frame.Duration > 0) aTimer.Interval = frame.Duration;
                else aTimer.Interval = 10;
                aTimer.Start();
            }
        }

        public int GetNextAnimationFrame()
        {
            if (currentFrame == null || currentAnimation == null)
                return 0;
            if (currentFrameIndex >= currentAnimation.Frames.Count)
                return 0;

            var branching = currentFrame.Branching;

            if (this._exiting && currentFrame.ExitBranch != null)
            {
                return int.Parse(currentFrame.ExitBranch);
            }

            if (branching != null)
            {
                var random = rnd.Next(0, 100);
                var branches = branching["branches"];

                for (var i = 0; i < branches.Length; i++)
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

        #endregion

    }

    public class AnimationStateEventArgs(string? name, AnimationStates states) : EventArgs
    {
        public string Name { get; } = name;
        public AnimationStates State { get; } = states;
    }
}
