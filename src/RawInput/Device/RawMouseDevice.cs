using System;

namespace Zergatul.Obs.InputOverlay.RawInput.Device
{
    public class RawMouseDevice : RawDevice
    {
        public int NumberOfButtons { get; }
        public int SampleRate { get; }

        internal RawMouseDevice(IntPtr hDevice, WinApi.User32.RID_DEVICE_INFO_MOUSE mouse)
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