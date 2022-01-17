using System;

namespace Zergatul.Obs.InputOverlay.Device
{
    public class RawMouseDevice : RawDevice
    {
        public int NumberOfButtons { get; }
        public int SampleRate { get; }

        internal RawMouseDevice(IntPtr hDevice, WinApi.RID_DEVICE_INFO_MOUSE mouse)
            : base (hDevice)
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