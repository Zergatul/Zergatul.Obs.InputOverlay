using Zergatul.Obs.InputOverlay.RawInput.Device;

namespace Zergatul.Obs.InputOverlay.Events
{
    public readonly struct AxisEvent
    {
        public RawGamepadDevice Gamepad { get; }
        public Axis Axis { get; }

        public AxisEvent(RawGamepadDevice gamepad, Axis axis)
        {
            Gamepad = gamepad;
            Axis = axis;
        }
    }
}