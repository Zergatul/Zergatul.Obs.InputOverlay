namespace Zergatul.Obs.InputOverlay.XInput
{
    public struct GamepadState
    {
        public int Index;
        public int Buttons;
        public int LeftTrigger;
        public int RightTrigger;
        public int LeftStickX;
        public int LeftStickY;
        public int RightStickX;
        public int RightStickY;

        internal GamepadState(int index, WinApi.XInput.XINPUT_STATE state)
        {
            Index = index;
            Buttons = state.Gamepad.wButtons;
            LeftTrigger = state.Gamepad.bLeftTrigger;
            RightTrigger = state.Gamepad.bRightTrigger;
            LeftStickX = state.Gamepad.sThumbLX;
            LeftStickY = state.Gamepad.sThumbLY;
            RightStickX = state.Gamepad.sThumbRX;
            RightStickY = state.Gamepad.sThumbRY;
        }
    }
}