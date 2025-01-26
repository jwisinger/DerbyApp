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
        public List<AgentAnimation>? Animations { get; }

        internal string? currentAnimationName;

        readonly Bitmap _bitmapSheet;
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

            if (agent.Model != null)
            {
                Animations = [];
                foreach (var animationKey in agent.Model.Animations)
                {
                    Animations.Add(new AgentAnimation(animationKey.Key, animationKey.Value));
                }
            }

            _bitmapSheet = GetImageSheet(name, "map.png");

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

        public bool HasAnimation(string name)
        {
            if (Animations == null) return false;
            return Animations.Any(s => s.Name == name);
        }

        public bool ShowAnimation(AgentAnimation animation)
        {
            _exiting = false;
            currentAnimation = animation;
            currentAnimationName = animation.Name;
            currentFrame = null;
            currentFrameIndex = 0;

            if (!_started)
            {
                Step();
                _started = true;
            }

            return true;
        }
        
        public bool ShowAnimation(string animationName)
        {
            _exiting = false;
            if (!HasAnimation(animationName))
            {
                return false;
            }

            currentAnimation = Animations?.FirstOrDefault(s => s.Name == animationName);
            currentAnimationName = animationName;
            currentFrame = null;
            currentFrameIndex = 0;

            if (!_started)
            {
                Step();
                _started = true;
            }

            return true;
        }

        public AgentAnimation? GetIdleAnimation()
        {
            if (Animations == null) return null;

            var r = new List<AgentAnimation>();
            foreach (var animation in Animations)
            {
                if (animation.IsIdle())
                {
                    r.Add(animation);
                }
            }

            return r[rnd.Next(0, r.Count - 1)];
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

        public ImageSource? GetImage(int[][] image)
        {
            if (image == null) return null;

            ImageSource? imageView = null;

            if (agent.ImageSize != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Bitmap img = new((int)agent.ImageSize.Width, (int)agent.ImageSize.Height);
                    using (Graphics gr = Graphics.FromImage(img))
                    {
                        for (int i = 0; i < image.Length; i++)
                        {
                            RectangleF cropRect = new(image[i][0], image[i][1], (int)agent.ImageSize.Width, (int)agent.ImageSize.Height);
                            gr.DrawImage(_bitmapSheet, new RectangleF(0, 0, img.Width, img.Height), cropRect, GraphicsUnit.Pixel);
                        }
                    }

                    using MemoryStream memory = new();
                    img.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    imageView = bitmapImage;
                });
            }

            return imageView;
        }

        public ImageSource? GetCurrentImage()
        {
            if (currentFrame == null) return null;
            return GetImage(currentFrame.Images);
        }
    }
}
