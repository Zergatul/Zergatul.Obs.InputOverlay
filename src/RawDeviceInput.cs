using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Zergatul.Obs.InputOverlay.Device;
using Zergatul.Obs.InputOverlay.Events;
using Zergatul.Obs.InputOverlay.Keyboard;
using Zergatul.Obs.InputOverlay.Mouse;

namespace Zergatul.Obs.InputOverlay
{
    using static WinApi;
    using static WinApiHelper;

    internal class RawDeviceInput : IRawDeviceInput
    {
        private const string WindowClass = "Input Overlay Hidden Window Class";
        private const string WindowName = "Input Overlay Hidden Window Name";
        private const int BufferSize = 1024;

        public event Action<ButtonEvent> ButtonAction;
        public event Action<MoveEvent> MoveAction;

        private readonly IRawDeviceFactory _factory;
        private readonly ILogger _logger;
        private readonly WndProc _wndProc;
        private readonly Thread _wndThread;
        private IntPtr _hWnd;
        private IntPtr _buffer;
        private int _rawInputHeaderSize;
        private int _rawHidDataOffset;
        private Dictionary<IntPtr, RawDevice> _devices;

        public RawDeviceInput(IRawDeviceFactory factory, ILogger<RawDeviceInput> logger)
        {
            _factory = factory;
            _logger = logger;
            _wndProc = WndProc;

            _buffer = Marshal.AllocHGlobal(BufferSize);
            _rawInputHeaderSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
            _rawHidDataOffset = Marshal.OffsetOf<RAWINPUT>(nameof(RAWINPUT.hid)).ToInt32() + Marshal.SizeOf<RAWHID>();
            _devices = new Dictionary<IntPtr, RawDevice>();

            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    _logger?.LogWarning("App is running under non-admin. Devices input will not work from applications running as admin.");
            }

            _wndThread = new Thread(ThreadFunc);
            _wndThread.Start();
        }

        public void Dispose()
        {
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

            _logger?.LogDebug("Disposed.");
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
                _logger?.LogError($"Cannot register window class {FormatWin32Error(Marshal.GetLastWin32Error())}.");
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
                _logger?.LogError($"Cannot create window {FormatWin32Error(Marshal.GetLastWin32Error())}.");
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
                _logger?.LogError($"Cannot register raw mouse input {FormatWin32Error(Marshal.GetLastWin32Error())}.");
            }

            devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = RawInputDeviceUsagePage.HID_USAGE_PAGE_GENERIC;
            devices[0].usUsage = RawInputDeviceUsage.HID_USAGE_GENERIC_KEYBOARD;
            devices[0].dwFlags = RawInputDeviceFlags.RIDEV_INPUTSINK | RawInputDeviceFlags.RIDEV_DEVNOTIFY;
            devices[0].hwndTarget = _hWnd;
            if (!RegisterRawInputDevices(devices, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                _logger?.LogError($"Cannot register raw keyboard input {FormatWin32Error(Marshal.GetLastWin32Error())}.");
            }

            devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = RawInputDeviceUsagePage.HID_USAGE_PAGE_GENERIC;
            devices[0].usUsage = RawInputDeviceUsage.HID_USAGE_GENERIC_GAMEPAD;
            devices[0].dwFlags = RawInputDeviceFlags.RIDEV_INPUTSINK | RawInputDeviceFlags.RIDEV_DEVNOTIFY;
            devices[0].hwndTarget = _hWnd;
            if (!RegisterRawInputDevices(devices, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                _logger?.LogError($"Cannot register raw gamepad input {FormatWin32Error(Marshal.GetLastWin32Error())}.");
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
                _logger?.LogError($"GetRawInputData error {FormatWin32Error(Marshal.GetLastWin32Error())}.");
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
                    byte[] bytes = new byte[data.hid.dwCount * data.hid.dwSizeHid];
                    Marshal.Copy(buffer + _rawHidDataOffset, bytes, 0, bytes.Length);
                    var sb = new System.Text.StringBuilder(bytes.Length * 2);
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (bytes[i] < 16)
                        {
                            sb.Append('0');
                            sb.Append(bytes[i].ToString("x2"));
                        }
                        else
                        {
                            sb.Append(bytes[i].ToString("x2"));
                        }
                    }
                    _logger?.LogDebug($"HID=" + sb.ToString());
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
                _logger?.LogInformation($"Device added. {device}.");
                _devices.Add(lParam, device);
            }
            if (wParam.ToInt64() == GIDC_REMOVAL)
            {
                if (_devices.TryGetValue(lParam, out var device))
                {
                    _logger?.LogInformation($"Device removed. {device}.");
                    _devices.Remove(lParam);
                }
                else
                {
                    _logger?.LogInformation($"Device removed, but not present in the dictionary.");
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
    }
}