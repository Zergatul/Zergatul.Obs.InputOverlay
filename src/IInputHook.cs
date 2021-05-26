using System;

namespace Zergatul.Obs.InputOverlay
{
    public interface IInputHook : IDisposable
    {
        event EventHandler<ButtonEvent> ButtonAction;
    }
}