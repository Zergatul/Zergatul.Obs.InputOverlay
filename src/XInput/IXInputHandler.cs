using System;

namespace Zergatul.Obs.InputOverlay.XInput
{
    public interface IXInputHandler : IDisposable
    {
        event Action<GamepadState> OnStateChanged;
    }
}