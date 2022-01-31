using System;

namespace Zergatul.Obs.InputOverlay.RawInput.Device
{
    public interface IRawDeviceFactory : IDisposable
    {
        RawDevice FromHDevice(IntPtr hDevice);
    }
}