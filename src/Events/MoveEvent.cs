namespace Zergatul.Obs.InputOverlay.Events
{
    public readonly struct MoveEvent
    {
        public MoveEventSource Source { get; }
        public int X { get; }
        public int Y { get; }

        public MoveEvent(MoveEventSource source, int x, int y)
        {
            Source = source;
            X = x;
            Y = y;
        }
    }
}