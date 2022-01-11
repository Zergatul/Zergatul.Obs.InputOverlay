using System;

namespace Zergatul.Obs.InputOverlay.Device
{
    public interface IRawDeviceFactory
    {
        RawDevice FromHDevice(IntPtr hDevice);
    }
}