namespace Zergatul.Obs.InputOverlay.Device
{
    public class RawMouseDevice : RawDevice
    {
        public int NumberOfButtons { get; }
        public int SampleRate { get; }

        internal RawMouseDevice(WinApi.RID_DEVICE_INFO_MOUSE mouse)
        {
            NumberOfButtons = mouse.dwNumberOfButtons;
            SampleRate = mouse.dwSampleRate;
        }

        public override string ToString()
        {
            return $"Mouse: NumberOfButtons={NumberOfButtons} SampleRate={SampleRate}";
        }
    }
}