namespace Zergatul.Obs.InputOverlay.RawInput.Device
{
    public class Axis
    {
        public int Index { get; }
        public ushort UsagePage { get; }
        public int BitSize { get; }
        public int BitMask { get; }
        public bool IsAbsolute { get; }
        public int LinkCollection { get; }
        public bool HasNull { get; }
        public int LogicalMin { get; }
        public int LogicalMax { get; }
        public ushort UsageMin { get; }
        public uint Value { get; set; }
        public bool Ignore { get; set; }

        internal Axis(int index, WinApi.Hid.HIDP_VALUE_CAPS caps)
        {
            Index = index;
            UsagePage = (ushort)caps.UsagePage;
            BitSize = caps.BitSize;
            BitMask = (1 << caps.BitSize) - 1;
            IsAbsolute = caps.IsAbsolute != 0;
            LinkCollection = caps.LinkCollection;
            HasNull = caps.HasNull != 0;
            LogicalMin = caps.LogicalMin;
            LogicalMax = caps.LogicalMax;

            UsageMin = caps.Range.UsageMin;
        }
    }
}