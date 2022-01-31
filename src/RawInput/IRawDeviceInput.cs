using System;
using System.Collections.Generic;
using Zergatul.Obs.InputOverlay.Events;
using Zergatul.Obs.InputOverlay.RawInput.Device;

namespace Zergatul.Obs.InputOverlay.RawInput
{
    public interface IRawDeviceInput : IDisposable
    {
        IReadOnlyDictionary<IntPtr, RawDevice> Devices { get; }
        event Action<ButtonEvent> ButtonAction;
        event Action<MoveEvent> MoveAction;
        event Action<AxisEvent> AxisAction;
        event Action<DeviceEvent> DeviceAction;
    }
}