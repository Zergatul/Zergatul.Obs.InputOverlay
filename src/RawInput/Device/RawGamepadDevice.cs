using System;
using System.Collections.Generic;

namespace Zergatul.Obs.InputOverlay.RawInput.Device
{
    public class RawGamepadDevice : RawHidDevice, IDisposable
    {
        public NativeMemoryBlock PreparsedData { get; }
        public int ButtonsCount { get; }
        public IReadOnlyDictionary<int, Axis> Axes { get; }
        public IReadOnlyList<GamepadButton> Buttons { get; }

        internal RawGamepadDevice(
            IntPtr hDevice,
            WinApi.User32.RID_DEVICE_INFO_HID hid,
            NativeMemoryBlock preparsedData,
            int buttonsCount,
            IReadOnlyDictionary<int, Axis> axes)
            : base(hDevice, hid)
        {
            if (preparsedData == null)
            {
                throw new ArgumentNullException(nameof(preparsedData));
            }

            PreparsedData = preparsedData;
            ButtonsCount = buttonsCount;
            Axes = axes;

            var buttons = new GamepadButton[ButtonsCount];
            for (int i = 0; i < buttonsCount; i++)
            {
                buttons[i] = new GamepadButton(i);
            }
            Buttons = buttons;
        }

        public void Dispose()
        {
            PreparsedData.Dispose();
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return "Gamepad";
        }
    }
}