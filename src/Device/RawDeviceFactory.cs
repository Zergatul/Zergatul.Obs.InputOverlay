using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Zergatul.Obs.InputOverlay.Device
{
    using static WinApi;
    using static WinApiHelper;

    public class RawDeviceFactory : IRawDeviceFactory
    {
        private readonly ILogger _logger;

        public RawDeviceFactory(ILogger<RawDeviceFactory> logger)
        {
            _logger = logger;
        }

        public RawDevice FromHDevice(IntPtr hDevice)
        {
            RID_DEVICE_INFO info = default;
            int size = Marshal.SizeOf<RID_DEVICE_INFO>();
            if (GetRawInputDeviceInfoW(hDevice, GetRawDeviceInfoCommand.RIDI_DEVICEINFO, ref info, ref size) < 0)
            {
                _logger?.LogError($"Cannot get raw input device info {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return null;
            }

            size = 0;
            if (GetRawInputDeviceInfoW(hDevice, GetRawDeviceInfoCommand.RIDI_DEVICENAME, null, ref size) < 0)
            {
                _logger?.LogError($"Cannot get raw input device name length {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return null;
            }

            var sb = new StringBuilder(size);
            if (GetRawInputDeviceInfoW(hDevice, GetRawDeviceInfoCommand.RIDI_DEVICENAME, sb, ref size) < 0)
            {
                _logger?.LogError($"Cannot get raw input device name {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return null;
            }

            string devicePath = sb.ToString();
            IntPtr handle = CreateFileW(
                devicePath,
                AccessMask.Zero,
                FileShare.FILE_SHARE_READ | FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero,
                CreationDisposition.OPEN_EXISTING,
                0,
                IntPtr.Zero);

            if (handle == InvalidHandle)
            {
                _logger?.LogError($"Cannot create device file {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return null;
            }

            string product;

            try
            {
                sb = new StringBuilder(1024);
                if (!HidD_GetProductString(handle, sb, 1024))
                {
                    _logger?.LogError($"Cannot get product string {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                    return null;
                }

                product = sb.ToString();
            }
            finally
            {
                CloseHandle(handle);
            }

            _logger?.LogDebug("Product=" + product);

            return info.dwType switch
            {
                RawInputType.RIM_TYPEMOUSE => new RawMouseDevice(info.mouse),
                RawInputType.RIM_TYPEKEYBOARD => new RawKeyboardDevice(info.keyboard),
                RawInputType.RIM_TYPEHID => info.hid.usUsage switch
                {
                    RawInputDeviceUsage.HID_USAGE_GENERIC_GAMEPAD => new RawGamepadDevice(info.hid),
                    _ => null,
                },
                _ => throw new InvalidOperationException("Invalid dwType."),
            };
        }
    }
}