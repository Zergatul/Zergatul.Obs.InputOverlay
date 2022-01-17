using Zergatul.Obs.InputOverlay.Device;

namespace Zergatul.Obs.InputOverlay
{
    public readonly struct DeviceEvent
    {
        public RawDevice Device { get; }
        public bool Attached { get; }

        public DeviceEvent(RawDevice device, bool attached)
        {
            Device = device;
            Attached = attached;
        }
    }
}