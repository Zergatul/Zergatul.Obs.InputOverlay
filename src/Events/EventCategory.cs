namespace Zergatul.Obs.InputOverlay.Events
{
    public enum EventCategory : int
    {
        Keyboard = 0x0001,
        MouseButtons = 0x0002,
        RawMouseMovement = 0x0004, /* Raw mouse data from driver */
        MouseMovement = 0x0008     /* Not supported */,
        RawInputDevices = 0x0010,
        RawInputGamepadButtons = 0x0020,
        RawInputGamepadAxes = 0x0040,
        XInputGamepadState = 0x0080,
    }
}