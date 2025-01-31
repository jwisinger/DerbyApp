namespace ClippySharp
{
    public enum AnimationStates
    {
        Waiting = 1,
        Exited = 0
    }

    public class AnimationStateEventArgs(string? name, AnimationStates states) : EventArgs
    {
        public string? Name { get; } = name;
        public AnimationStates State { get; } = states;
    }
}
