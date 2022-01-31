namespace Zergatul.Obs.InputOverlay.Events
{
    public readonly struct RawKeyboardEvent
    {
        public int MakeCode { get; }
        public int Flags { get; }
        public int VKey { get; }

        internal RawKeyboardEvent(WinApi.User32.RAWKEYBOARD raw)
        {
            MakeCode = raw.MakeCode;
            Flags = (int)raw.Flags;
            VKey = raw.VKey;
        }
    }
}