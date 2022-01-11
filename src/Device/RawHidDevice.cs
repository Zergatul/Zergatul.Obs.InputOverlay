namespace Zergatul.Obs.InputOverlay.Device
{
    public abstract class RawHidDevice : RawDevice
    {
        public int VendorId { get; }
        public int ProductId { get; }
        public int VersionNumber { get; }

        internal RawHidDevice(WinApi.RID_DEVICE_INFO_HID hid)
        {
            VendorId = hid.dwVendorId;
            ProductId = hid.dwProductId;
            VersionNumber = hid.dwVersionNumber;
        }
    }
}