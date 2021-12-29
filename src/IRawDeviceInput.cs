using System;
using Zergatul.Obs.InputOverlay.Events;

namespace Zergatul.Obs.InputOverlay
{
    public interface IRawDeviceInput : IDisposable
    {
        event Action<ButtonEvent> ButtonAction;
        event Action<MoveEvent> MoveAction;
    }
}