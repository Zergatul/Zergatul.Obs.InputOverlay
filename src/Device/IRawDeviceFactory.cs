using System;

namespace Zergatul.Obs.InputOverlay.Device
{
    public interface IRawDeviceFactory : IDisposable
    {
        RawDevice FromHDevice(IntPtr hDevice);
    }
}