using System;

namespace Zergatul.Obs.InputOverlay.RawInput.Device
{
    public class RawKeyboardDevice : RawDevice
    {
        public int NumberOfKeys { get; }

        internal RawKeyboardDevice(IntPtr hDevice, WinApi.User32.RID_DEVICE_INFO_KEYBOARD keyboard)
            : base(hDevice)
        {
            NumberOfKeys = keyboard.dwNumberOfKeysTotal;
        }

        public override string ToString()
        {
            return $"Keyboard: NumberOfKeys={NumberOfKeys}";
        }
    }
}