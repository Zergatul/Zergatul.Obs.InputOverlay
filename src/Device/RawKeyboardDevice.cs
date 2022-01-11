namespace Zergatul.Obs.InputOverlay.Device
{
    public class RawKeyboardDevice : RawDevice
    {
        public int NumberOfKeys { get; }

        internal RawKeyboardDevice(WinApi.RID_DEVICE_INFO_KEYBOARD keyboard)
        {
            NumberOfKeys = keyboard.dwNumberOfKeysTotal;
        }

        public override string ToString()
        {
            return $"Keyboard: NumberOfKeys={NumberOfKeys}";
        }
    }
}