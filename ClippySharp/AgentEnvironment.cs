using System.IO;

namespace ClippySharp
{
    public interface ISoundPlayer
    {
        ISoundPlayer Play(SoundData data, int delay = 500);
        ISoundPlayer Sleep(int miliseconds);
    }

    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }

        public static Point Zero { get; set; } = new Point();
    }

    public class Size
    {
        public Size()
        {

        }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public float Width { get; set; }
        public float Height { get; set; }

        public static Size Zero { get; set; } = new Size();
    }

    public class Rectangle
    {
        public Point Position = new();
        public Size Size = new();

        public Rectangle()
        {
        }

        public Rectangle(float x, float y, float width, float height)
        {
            Position.X = x;
            Position.Y = y;
            Size.Width = width;
            Size.Height = height;
        }

        public float X
        {
            get => Position.X;
            set => Position.X = value;
        }

        public float Y
        {
            get => Position.Y;
            set => Position.Y = value;
        }
        public float Width
        {
            get => Size.Width;
            set => Size.Width = value;
        }
        public float Height
        {
            get => Size.Height;
            set => Size.Height = value;
        }

        public static Rectangle Zero { get; set; } = new Rectangle { X = 0, Y = 0, Width = 0, Height = 0 };

        public float Top => Y;
        public float Left => X + Width;
    }

    public class AgentEnvironment
    {
        AgentEnvironment()
        {

        }


        //TODO: change this to use a directory thing
        static readonly List<string[]> agents = [
            [ "bonzi", "BonziBuddy" ], ["clippy", "Clippy"], ["f1", "F1 Robot"], ["genius", "Einstein"], ["links", "Links the Cat"], ["merlin", "Merlin"], ["peedy", "Peedy"], ["rocky", "Rocky"],[ "rover", "Rover"]
        ];

        public static List<string[]> GetAgents()
        {
            return agents;
        }

        public ISoundPlayer? SoundPlayer { get; private set; }

        public void Initialize(ISoundPlayer soundPlayer)
        {
            SoundPlayer = soundPlayer;
        }

        static AgentEnvironment? current;
        public static AgentEnvironment Current
        {
            get
            {
                current ??= new AgentEnvironment();
                return current;
            }
        }
    }
}
