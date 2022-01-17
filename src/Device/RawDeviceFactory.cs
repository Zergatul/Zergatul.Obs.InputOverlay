using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Zergatul.Obs.InputOverlay.Device
{
    using static WinApi;
    using static WinApiHelper;

    public class RawDeviceFactory : IRawDeviceFactory
    {
        private readonly ILogger _logger;
        private readonly NativeMemoryBuffer _buffer;

        public RawDeviceFactory(ILogger<RawDeviceFactory> logger)
        {
            _logger = logger;
            _buffer = new NativeMemoryBuffer();
        }

        public void Dispose()
        {
            _buffer.Dispose();
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

            /*string product;*/

            try
            {
                /*sb = new StringBuilder(1024);
                if (!HidD_GetProductString(handle, sb, sb.Capacity))
                {
                    _logger?.LogError($"Cannot get product string {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                    return null;
                }

                product = sb.ToString();

                if (info.dwType == RawInputType.RIM_TYPEHID)
                {
                    sb = new StringBuilder(1024);
                    if (!HidD_GetSerialNumberString(handle, sb, sb.Capacity))
                    {
                        _logger?.LogError($"Cannot get serial number {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                        return null;
                    }

                    string serial = sb.ToString();

                    sb = new StringBuilder(1024);
                    if (!HidD_GetManufacturerString(handle, sb, sb.Capacity))
                    {
                        _logger?.LogError($"Cannot get manufacturer string {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                        return null;
                    }
                }*/
            }
            finally
            {
                CloseHandle(handle);
            }

            return info.dwType switch
            {
                RawInputType.RIM_TYPEMOUSE => new RawMouseDevice(hDevice, info.mouse),
                RawInputType.RIM_TYPEKEYBOARD => new RawKeyboardDevice(hDevice, info.keyboard),
                RawInputType.RIM_TYPEHID => info.hid.usUsage switch
                {
                    RawInputDeviceUsage.HID_USAGE_GENERIC_GAMEPAD => CreateGamepadDevice(hDevice, info.hid),
                    _ => null,
                },
                _ => throw new InvalidOperationException("Invalid dwType."),
            };
        }

        private RawGamepadDevice CreateGamepadDevice(IntPtr hDevice, RID_DEVICE_INFO_HID hid)
        {
            int size = 0;
            if (GetRawInputDeviceInfoW(hDevice, GetRawDeviceInfoCommand.RIDI_PREPARSEDDATA, null, ref size) == -1)
            {
                _logger?.LogError($"Cannot get preparsed data size {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return null;
            }

            _buffer.EnsureCapacity(size);

            if (GetRawInputDeviceInfoW(hDevice, GetRawDeviceInfoCommand.RIDI_PREPARSEDDATA, _buffer.Pointer, ref size) == -1)
            {
                _logger?.LogError($"Cannot get preparsed data {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return null;
            }

            NativeMemoryBlock preparsedData = _buffer.ToMemoryBlock(size);

            HIDP_CAPS caps = default;
            if (HidP_GetCaps(preparsedData.Pointer, ref caps) != HidPStatus.HIDP_STATUS_SUCCESS)
            {
                _logger?.LogError($"Cannot get gamepad caps {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return null;
            }

            ushort numberButtonsCaps = caps.NumberInputButtonCaps;
            HIDP_BUTTON_CAPS[] buttonCaps = new HIDP_BUTTON_CAPS[numberButtonsCaps];
            if (HidP_GetButtonCaps(HIDP_REPORT_TYPE.HidP_Input, buttonCaps, ref numberButtonsCaps, preparsedData.Pointer) != HidPStatus.HIDP_STATUS_SUCCESS)
            {
                _logger?.LogError($"Cannot get gamepad button caps {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return null;
            }

            int buttonsCount = 0;
            for (int i = 0; i < caps.NumberInputButtonCaps; i++)
            {
                if (buttonCaps[i].UsagePage == RawInputDeviceUsagePage.HID_USAGE_PAGE_BUTTON)
                {
                    buttonsCount = Math.Max(buttonsCount, buttonCaps[i].Range.UsageMax);
                }
            }

            ushort numberValueCaps = caps.NumberInputValueCaps;
            HIDP_VALUE_CAPS[] valueCaps = new HIDP_VALUE_CAPS[numberValueCaps];
            if (HidP_GetValueCaps(HIDP_REPORT_TYPE.HidP_Input, valueCaps, ref numberValueCaps, preparsedData.Pointer) != HidPStatus.HIDP_STATUS_SUCCESS)
            {
                _logger?.LogError($"Cannot get gamepad value caps {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return null;
            }

            var axes = new Dictionary<int, Axis>();
            for (int i = 0; i < caps.NumberInputValueCaps; i++)
            {
                int index = valueCaps[i].Range.UsageMin - 0x30;
                axes.Add(index, new Axis(index, valueCaps[i]));
            }

            return new RawGamepadDevice(hDevice, hid, preparsedData, buttonsCount, axes);
        }
    }
}