using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Zergatul.Obs.InputOverlay.Events;
using Zergatul.Obs.InputOverlay.Keyboard;
using Zergatul.Obs.InputOverlay.Mouse;
using Zergatul.Obs.InputOverlay.RawInput.Device;

namespace Zergatul.Obs.InputOverlay.RawInput
{
    using static WinApi.Kernel32;
    using static WinApi.User32;
    using static WinApi.Hid;
    using static WinApiHelper;

    internal class RawDeviceInput : IRawDeviceInput
    {
        private const string WindowClass = "Input Overlay Hidden Window Class";
        private const string WindowName = "Input Overlay Hidden Window Name";
        private const int BufferSize = 1024;

        public IReadOnlyDictionary<IntPtr, RawDevice> Devices => _devices;
        public event Action<ButtonEvent> ButtonAction;
        public event Action<MoveEvent> MoveAction;
        public event Action<AxisEvent> AxisAction;
        public event Action<DeviceEvent> DeviceAction;

        private readonly IRawDeviceFactory _factory;
        private readonly ILogger _logger;
        private readonly WndProc _wndProc;
        private readonly Thread _wndThread;
        private IntPtr _hWnd;
        private IntPtr _buffer;
        private int _rawInputHeaderSize;
        private int _rawHidDataOffset;
        private Dictionary<IntPtr, RawDevice> _devices;
        private bool[] _gamepadButtonsBuffer;

        public RawDeviceInput(IRawDeviceFactory factory, ILogger<RawDeviceInput> logger)
        {
            _factory = factory;
            _logger = logger;
            _wndProc = WndProc;

            _buffer = Marshal.AllocHGlobal(BufferSize);
            _rawInputHeaderSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
            _rawHidDataOffset = Marshal.OffsetOf<RAWINPUT>(nameof(RAWINPUT.hid)).ToInt32() + Marshal.SizeOf<RAWHID>();
            _devices = new Dictionary<IntPtr, RawDevice>();
            _gamepadButtonsBuffer = new bool[256];

            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    _logger.LogWarning("App is running under non-admin. Devices input will not work from applications running as admin.");
            }

            _wndThread = new Thread(ThreadFunc);
            _wndThread.Start();
        }

        public void Dispose()
        {
            _factory?.Dispose();

            if (_hWnd != IntPtr.Zero)
            {
                SendMessage(_hWnd, WindowsMessage.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
                _wndThread.Join();
                _hWnd = IntPtr.Zero;
            }

            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
            }

            _logger.LogDebug("Disposed.");
        }

        private void ThreadFunc()
        {
            IntPtr hInstance = GetModuleHandleW(null);

            WNDCLASSEX wc = new WNDCLASSEX();
            wc.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
            wc.lpfnWndProc = _wndProc;
            wc.hInstance = hInstance;
            wc.lpszClassName = WindowClass;
            ushort atom = RegisterClassEx(ref wc);
            if (atom == 0)
            {
                _logger.LogError($"Cannot register window class {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return;
            }

            _hWnd = CreateWindowExW(
                dwExStyle: 0,
                lpClassName: new IntPtr(atom),
                lpWindowName: WindowName,
                dwStyle: WindowStyles.WS_POPUP,
                x: 0,
                y: 0,
                nWidth: 100,
                nHeight: 100,
                hWndParent: IntPtr.Zero,
                hMenu: IntPtr.Zero,
                hInstance: hInstance,
                lpParam: IntPtr.Zero);
            if (_hWnd == IntPtr.Zero)
            {
                _logger.LogError($"Cannot create window {FormatWin32Error(Marshal.GetLastWin32Error())}.");
                return;
            }

            RAWINPUTDEVICE[] devices;

            devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = RawInputDeviceUsagePage.HID_USAGE_PAGE_GENERIC;
            devices[0].usUsage = RawInputDeviceUsage.HID_USAGE_GENERIC_MOUSE;
            devices[0].dwFlags = RawInputDeviceFlags.RIDEV_INPUTSINK | RawInputDeviceFlags.RIDEV_DEVNOTIFY;
            devices[0].hwndTarget = _hWnd;
            if (!RegisterRawInputDevices(devices, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                _logger.LogError($"Cannot register raw mouse input {FormatWin32Error(Marshal.GetLastWin32Error())}.");
            }

            devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = RawInputDeviceUsagePage.HID_USAGE_PAGE_GENERIC;
            devices[0].usUsage = RawInputDeviceUsage.HID_USAGE_GENERIC_KEYBOARD;
            devices[0].dwFlags = RawInputDeviceFlags.RIDEV_INPUTSINK | RawInputDeviceFlags.RIDEV_DEVNOTIFY;
            devices[0].hwndTarget = _hWnd;
            if (!RegisterRawInputDevices(devices, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                _logger.LogError($"Cannot register raw keyboard input {FormatWin32Error(Marshal.GetLastWin32Error())}.");
            }

            devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = RawInputDeviceUsagePage.HID_USAGE_PAGE_GENERIC;
            devices[0].usUsage = RawInputDeviceUsage.HID_USAGE_GENERIC_GAMEPAD;
            devices[0].dwFlags = RawInputDeviceFlags.RIDEV_INPUTSINK | RawInputDeviceFlags.RIDEV_DEVNOTIFY;
            devices[0].hwndTarget = _hWnd;
            if (!RegisterRawInputDevices(devices, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                _logger.LogError($"Cannot register raw gamepad input {FormatWin32Error(Marshal.GetLastWin32Error())}.");
            }

            while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        private IntPtr WndProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WindowsMessage.WM_INPUT:
                    ProcessWmInputMessage(lParam);
                    break;

                case WindowsMessage.WM_INPUT_DEVICE_CHANGE:
                    ProcessWmInputDeviceChangeMessage(wParam, lParam);
                    break;

                case WindowsMessage.WM_DESTROY:
                    PostQuitMessage(0);
                    break;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private void ProcessWmInputMessage(IntPtr lParam)
        {
            IntPtr buffer = _buffer;
            int size = BufferSize;
            if (GetRawInputData(lParam, GetRawInputDataCommand.RID_INPUT, buffer, ref size, _rawInputHeaderSize) == -1)
            {
                _logger.LogError($"GetRawInputData error {FormatWin32Error(Marshal.GetLastWin32Error())}.");
            }
            else
            {
                RAWINPUT data = Marshal.PtrToStructure<RAWINPUT>(buffer);

                if (data.header.dwType == RawInputType.RIM_TYPEKEYBOARD)
                {
                    ProcessKeyboardEvent(data.keyboard);
                }

                if (data.header.dwType == RawInputType.RIM_TYPEMOUSE)
                {
                    ProcessMouseEvent(data.mouse);
                }

                if (data.header.dwType == RawInputType.RIM_TYPEHID)
                {
                    ProcessHidEvent(data.header.hDevice, data.hid);
                }
            };
        }

        private void ProcessWmInputDeviceChangeMessage(IntPtr wParam, IntPtr lParam)
        {
            const int GIDC_ARRIVAL = 1;
            const int GIDC_REMOVAL = 2;
            if (wParam.ToInt64() == GIDC_ARRIVAL)
            {
                RawDevice device = _factory.FromHDevice(lParam);
                if (device is RawGamepadDevice gamepad)
                {
                    DeviceAction?.Invoke(new DeviceEvent(device, true));
                    _logger.LogInformation($"Gamepad added.\n\t\t" +
                        $"HDevice={gamepad.HDeviceStr}\n\t\t" +
                        $"VendorId={FormatInt16(gamepad.VendorId)}\n\t\t" +
                        $"Vendor={gamepad.VendorName}\n\t\t" +
                        $"ProductId={FormatInt16(gamepad.ProductId)}\n\t\t" +
                        $"Product={gamepad.ProductName}");

                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        var axesInfo = new List<string>(gamepad.Axes.Count);
                        foreach (var (index, axis) in gamepad.Axes)
                        {
                            axesInfo.Add($"axis#{index}: collection={axis.LinkCollection} min={axis.LogicalMin & axis.BitMask} max={axis.LogicalMax & axis.BitMask} hasnull={axis.HasNull}");
                        }
                        _logger.LogDebug($"Gamepad information.\n\t\t" +
                            $"ButtonsCount={gamepad.ButtonsCount}\n\t\t" +
                            $"AxesCount={gamepad.Axes.Count}\n\t\t\t" +
                            string.Join("\n\t\t\t", axesInfo));
                    }
                }
                lock (_devices)
                {
                    _devices.Add(lParam, device);
                }
            }
            if (wParam.ToInt64() == GIDC_REMOVAL)
            {
                lock (_devices)
                {
                    if (_devices.TryGetValue(lParam, out var device))
                    {
                        if (device is RawGamepadDevice)
                        {
                            DeviceAction?.Invoke(new DeviceEvent(device, false));
                            _logger.LogInformation($"Gamepad removed.\n\t\t" +
                                $"HDevice={device.HDeviceStr}");
                        }
                        _devices.Remove(lParam);
                    }
                    else
                    {
                        _logger.LogWarning($"Device removed, but not present in the dictionary.");
                    }
                }
            }
        }

        private void ProcessKeyboardEvent(RAWKEYBOARD keyboard)
        {
            int code = keyboard.MakeCode;
            if (code < 0x0100 && (keyboard.Flags & RawKeyboardFlags.RI_KEY_E0) != 0)
            {
                code |= 0xE000;
            }
            if (code < 0x0100 && (keyboard.Flags & RawKeyboardFlags.RI_KEY_E1) != 0)
            {
                code |= 0xE100;
            }
            bool pressed = (keyboard.Flags & RawKeyboardFlags.RI_KEY_BREAK) == 0;
            if (!KeyboardMapping.Dictionary.TryGetValue(code, out KeyboardButton button))
            {
                button = KeyboardButton.Unknown;
            }
            ButtonAction?.Invoke(new ButtonEvent(button, new RawKeyboardEvent(keyboard), pressed));
        }

        private void ProcessMouseEvent(RAWMOUSE mouse)
        {
            if (mouse.usFlags == RawMouseFlags.MOUSE_MOVE_RELATIVE)
            {
                int x = mouse.lLastX;
                int y = mouse.lLastY;
                if (x != 0 || y != 0)
                {
                    MoveAction?.Invoke(new MoveEvent(MoveEventSource.RawMouse, x, y));
                }
            }

            if (mouse.usButtonFlags != 0)
            {
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_1_DOWN))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse1, true));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_1_UP))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse1, false));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_2_DOWN))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse2, true));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_2_UP))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse2, false));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_3_DOWN))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse3, true));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_3_UP))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse3, false));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_4_DOWN))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse4, true));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_4_UP))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse4, false));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_5_DOWN))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse5, true));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_BUTTON_5_UP))
                {
                    ButtonAction?.Invoke(new ButtonEvent(MouseButton.Mouse5, false));
                }
                if (mouse.usButtonFlags.HasFlag(RawMouseButtonsFlags.RI_MOUSE_WHEEL))
                {
                    int delta = unchecked((short)mouse.usButtonData) / 120;
                    if (delta > 0)
                    {
                        ButtonAction?.Invoke(new ButtonEvent(MouseButton.MouseWheelUp, delta));
                    }
                    if (delta < 0)
                    {
                        ButtonAction?.Invoke(new ButtonEvent(MouseButton.MouseWheelDown, -delta));
                    }
                }
            }
        }

        private void ProcessHidEvent(IntPtr hDevice, RAWHID hid)
        {
            RawDevice device;
            lock (_devices)
            {
                if (!_devices.TryGetValue(hDevice, out device))
                {
                    _logger.LogWarning($"Cannot find device by hDevice={FormatIntPtr(hDevice)}.");
                    return;
                }
            }

            if (device is not RawGamepadDevice gamepad)
            {
                _logger.LogWarning($"RawHid event invalid device: {device.GetType()}.");
                return;
            }

            IntPtr reportPointer = _buffer + _rawHidDataOffset;

            if (gamepad.ButtonsCount > 0)
            {
                Array.Clear(_gamepadButtonsBuffer, 0, gamepad.ButtonsCount);

                HidPStatus status;

                uint buttonsLength = default;
                status = HidP_GetUsagesEx(HIDP_REPORT_TYPE.HidP_Input, 0, null, ref buttonsLength, gamepad.PreparsedData.Pointer, reportPointer, hid.dwSizeHid);

                if (status == HidPStatus.HIDP_STATUS_BUFFER_TOO_SMALL)
                {
                    // TODO: stop allocation
                    USAGE_AND_PAGE[] usages = new USAGE_AND_PAGE[buttonsLength];
                    status = HidP_GetUsagesEx(HIDP_REPORT_TYPE.HidP_Input, 0, usages, ref buttonsLength, gamepad.PreparsedData.Pointer, reportPointer, hid.dwSizeHid);
                    if (status != HidPStatus.HIDP_STATUS_SUCCESS)
                    {
                        _logger.LogWarning($"HidP_GetUsagesEx 2 failed. {status}.");
                        return;
                    }

                    for (int j = 0; j < buttonsLength; j++)
                    {
                        int buttonIndex = (int)usages[j].Usage - 1;
                        if (usages[j].UsagePage == RawInputDeviceUsagePage.HID_USAGE_PAGE_BUTTON && buttonIndex >= 0)
                        {
                            _gamepadButtonsBuffer[buttonIndex] = true;
                            if (!gamepad.Buttons[buttonIndex].Pressed)
                            {
                                gamepad.Buttons[buttonIndex].Pressed = true;
                                ButtonAction?.Invoke(new ButtonEvent(gamepad, buttonIndex, true));
                            }
                        }
                    }
                }
                else if (status == HidPStatus.HIDP_STATUS_SUCCESS)
                {
                    // zero buttons
                }
                else
                {
                    _logger.LogWarning($"HidP_GetUsagesEx 1 failed. {status}.");
                    return;
                }

                for (int i = 0; i < gamepad.ButtonsCount; i++)
                {
                    if (!_gamepadButtonsBuffer[i] && gamepad.Buttons[i].Pressed)
                    {
                        gamepad.Buttons[i].Pressed = false;
                        ButtonAction?.Invoke(new ButtonEvent(gamepad, i, false));
                    }
                }
            }

            if (gamepad.Axes.Count > 0)
            {
                foreach (var (_, axis) in gamepad.Axes)
                {
                    if (axis.Ignore)
                    {
                        continue;
                    }

                    if (axis.LogicalMin < 0)
                    {
                        _logger.LogWarning($"axis.LogicalMin < 0 not implemented.");
                    }
                    else
                    {
                        HidPStatus status = HidP_GetUsageValue(
                            HIDP_REPORT_TYPE.HidP_Input,
                            (RawInputDeviceUsagePage)axis.UsagePage,
                            0,
                            axis.UsageMin,
                            out uint value,
                            gamepad.PreparsedData.Pointer,
                            reportPointer,
                            hid.dwSizeHid);
                        if (status != HidPStatus.HIDP_STATUS_SUCCESS)
                        {
                            _logger.LogWarning($"HidP_GetUsageValue failed for axis #{axis.Index}. This axis will be ignored. {status}.");
                            axis.Ignore = true;
                            continue;
                        }

                        if (axis.IsAbsolute)
                        {
                            if (axis.Value != value)
                            {
                                axis.Value = value;
                                AxisAction?.Invoke(new AxisEvent(gamepad, axis));
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"axis.IsAbsolute=false not implemented.");
                        }
                    }
                }
            }
        }
    }
}