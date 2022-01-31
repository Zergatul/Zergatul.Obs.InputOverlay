using Zergatul.Obs.InputOverlay.Keyboard;
using Zergatul.Obs.InputOverlay.Mouse;
using Zergatul.Obs.InputOverlay.RawInput.Device;

namespace Zergatul.Obs.InputOverlay.Events
{
    public readonly struct ButtonEvent
    {
        public KeyboardButton KeyboardButton { get; }
        public RawKeyboardEvent RawKeyboard { get; }
        public MouseButton MouseButton { get; }
        public RawGamepadDevice Gamepad { get; }
        public int? GamepadButton { get; }
        public bool Pressed { get; }

        /// <summary>
        /// For mouse scrolling
        /// </summary>
        public int? Count { get; }

        public ButtonEvent(KeyboardButton button, RawKeyboardEvent rawKeyboard, bool pressed)
        {
            KeyboardButton = button;
            RawKeyboard = rawKeyboard;
            MouseButton = MouseButton.None;
            Gamepad = null;
            GamepadButton = null;
            Pressed = pressed;
            Count = null;
        }

        public ButtonEvent(MouseButton button, bool pressed)
        {
            KeyboardButton = KeyboardButton.None;
            RawKeyboard = default;
            MouseButton = button;
            Gamepad = null;
            GamepadButton = null;
            Pressed = pressed;
            Count = null;
        }

        public ButtonEvent(MouseButton button, int count)
        {
            KeyboardButton = KeyboardButton.None;
            RawKeyboard = default;
            MouseButton = button;
            Gamepad = null;
            GamepadButton = null;
            Pressed = false;
            Count = count;
        }

        public ButtonEvent(RawGamepadDevice gamepad, int button, bool pressed)
        {
            KeyboardButton = KeyboardButton.None;
            RawKeyboard = default;
            MouseButton = MouseButton.None;
            Gamepad = gamepad;
            GamepadButton = button;
            Pressed = pressed;
            Count = null;
        }
    }
}