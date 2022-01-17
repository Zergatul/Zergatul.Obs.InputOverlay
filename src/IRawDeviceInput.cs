using System;
using System.Collections.Generic;
using Zergatul.Obs.InputOverlay.Device;
using Zergatul.Obs.InputOverlay.Events;

namespace Zergatul.Obs.InputOverlay
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