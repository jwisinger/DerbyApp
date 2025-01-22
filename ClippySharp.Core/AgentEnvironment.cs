using System;
using System.IO;

namespace ClippySharp.Core
{
	public interface ISoundPlayer
	{
		ISoundPlayer Play (SoundData data, int delay = 500);
		ISoundPlayer Sleep (int miliseconds);
	}

	public class Point
	{
		public float X { get; set; }
		public float Y { get; set; }

		public static Point Zero { get; set; } = new Point ();
	}

	public class Size
	{
		public Size ()
		{

		}

		public Size (int width, int height)
		{
			Width = width;
			Height = height;
		}

		public float Width { get; set; }
		public float Height { get; set; }

		public static Size Zero { get; set; } = new Size ();
	}

	public class Rectangle
	{
		public Point Position = new Point ();
		public Size Size = new Size ();

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

		public float X {
			get => Position.X;
			set => Position.X = value;
		}

		public float Y {
			get => Position.Y;
			set => Position.Y = value;
		}
		public float Width {
			get => Size.Width;
			set => Size.Width = value;
		}
		public float Height {
			get => Size.Height;
			set => Size.Height = value;
		}

		public static Rectangle Zero { get; set; } = new Rectangle { X = 0, Y = 0, Width = 0, Height = 0 };

		public float Top => Y;
		public float Left => X + Width;
	}

	public interface IAgent : IDisposable
	{
		bool Hidden { get; }
		bool Sound { get; set; }
		Size ImageSize { get; }
		void PlaySound (string id);
		void Speak (string text, string hold);
		void Stop ();
		void Show (bool fast);
		bool GestureAt (float x, float y);
		void Pause ();
	}

	public interface IAgentDelegate
	{
		IImageWrapper GetImage (IBitmapWrapper imageWrapper, int [][] image, IAgent agent);

		IBitmapWrapper GetImageSheet (string agentName, string resourceName);

		IImageWrapper GetImage (Stream stream);
	}

	public class AgentEnvironment
	{
		AgentEnvironment ()
		{

		}


		//TODO: change this to use a directory thing
		static readonly string[][] agents = {
			[ "bonzi", "BonziBuddy" ], ["clippy", "Clippy"], ["f1", "F1 Robot"], ["genius", "Einstein"], ["links", "Links the Cat"], ["merlin", "Merlin"], ["peedy", "Peedy"], ["rocky", "Rocky"],[ "rover", "Rover"]
		};

		public string[][] GetAgents ()
		{
			return agents;
		}

		public IAgentDelegate Delegate { get; private set; }

		public ISoundPlayer SoundPlayer { get; private set; }

		public void Initialize (IAgentDelegate agentDelegate, ISoundPlayer soundPlayer)
		{
			Delegate = agentDelegate;
			SoundPlayer = soundPlayer;
		}

		static AgentEnvironment current;
		public static AgentEnvironment Current {
			get {
				if (current == null) {
					current = new AgentEnvironment ();
				}
				return current;
			}
		}
	}
}
