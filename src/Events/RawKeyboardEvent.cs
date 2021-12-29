namespace Zergatul.Obs.InputOverlay.Events
{
    using static WinApi;

    public readonly struct RawKeyboardEvent
    {
        public int MakeCode { get; }
        public int Flags { get; }
        public int VKey { get; }

        internal RawKeyboardEvent(RAWKEYBOARD raw)
        {
            MakeCode = raw.MakeCode;
            Flags = (int)raw.Flags;
            VKey = raw.VKey;
        }
    }
}