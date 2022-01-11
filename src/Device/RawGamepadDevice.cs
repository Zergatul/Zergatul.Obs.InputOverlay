namespace Zergatul.Obs.InputOverlay.Device
{
    public class RawGamepadDevice : RawHidDevice
    {
        internal RawGamepadDevice(WinApi.RID_DEVICE_INFO_HID hid)
            : base(hid)
        {

        }

        public override string ToString()
        {
            return "Gamepad";
        }
    }
}