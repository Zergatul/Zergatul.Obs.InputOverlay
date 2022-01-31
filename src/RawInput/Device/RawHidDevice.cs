using System;

namespace Zergatul.Obs.InputOverlay.RawInput.Device
{
    public abstract class RawHidDevice : RawDevice
    {
        public int VendorId { get; }
        public int ProductId { get; }
        public int VersionNumber { get; }
        public string SerialNumber { get; }
        public string VendorName { get; }
        public string ProductName { get; }

        internal RawHidDevice(IntPtr hDevice, WinApi.User32.RID_DEVICE_INFO_HID hid)
            : base(hDevice)
        {
            VendorId = hid.dwVendorId;
            ProductId = hid.dwProductId;
            VersionNumber = hid.dwVersionNumber;

            VendorName = VendorIdentifier.Get(hid.dwVendorId);
            ProductName = ProductIdentifier.Get(hid.dwVendorId, hid.dwProductId);
        }
    }
}