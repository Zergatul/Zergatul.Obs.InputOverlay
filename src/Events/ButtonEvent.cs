using Zergatul.Obs.InputOverlay.Keyboard;
using Zergatul.Obs.InputOverlay.Mouse;

namespace Zergatul.Obs.InputOverlay.Events
{
    public readonly struct ButtonEvent
    {
        public KeyboardButton KeyboardButton { get; }
        public RawKeyboardEvent RawKeyboard { get; }
        public MouseButton MouseButton { get; }
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
            Pressed = pressed;
            Count = null;
        }

        public ButtonEvent(MouseButton button, bool pressed)
        {
            KeyboardButton = KeyboardButton.None;
            RawKeyboard = default;
            MouseButton = button;
            Pressed = pressed;
            Count = null;
        }

        public ButtonEvent(MouseButton button, int count)
        {
            KeyboardButton = KeyboardButton.None;
            RawKeyboard = default;
            MouseButton = button;
            Pressed = false;
            Count = count;
        }
    }
}