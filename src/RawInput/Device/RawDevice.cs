using System;

namespace Zergatul.Obs.InputOverlay.RawInput.Device
{
    public abstract class RawDevice
    {
        public IntPtr HDevice { get; }
        public string HDeviceStr { get; }

        protected RawDevice(IntPtr hDevice)
        {
            HDevice = hDevice;
            HDeviceStr = WinApiHelper.FormatIntPtr(hDevice);
        }
    }
}