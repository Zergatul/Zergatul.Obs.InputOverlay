using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Zergatul.Obs.InputOverlay.Events;
using Zergatul.Obs.InputOverlay.Keyboard;
using Zergatul.Obs.InputOverlay.Mouse;

namespace Zergatul.Obs.InputOverlay
{
    using static WinApi;

    internal class RawDeviceInput : IRawDeviceInput
    {
        private const string WindowClass = "Input Overlay Hidden Window Class";
        private const string WindowName = "Input Overlay Hidden Window Name";

        public event Action<ButtonEvent> ButtonAction;
        public event Action<MoveEvent> MoveAction;

        private readonly ILogger _logger;
        private readonly WndProc _wndProc;
        private readonly Thread _wndThread;
        private IntPtr _hWnd;

        public RawDeviceInput(ILogger<RawDeviceInput> logger)
        {
            _logger = logger;
            _wndProc = WndProc;

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
                throw new WinApiException("Cannot register class.");
            }

            _logger?.LogDebug($"Register window class atom: {atom}.");

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
                throw new WinApiException("Cannot create window.");
            }

            _logger?.LogDebug($"Window handle: {_hWnd}.");

            RAWINPUTDEVICE[] devices;

            devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = RawInputDeviceUsagePage.HID_USAGE_PAGE_GENERIC;
            devices[0].usUsage = RawInputDeviceUsage.HID_USAGE_GENERIC_MOUSE;
            devices[0].dwFlags = RawInputDeviceFlags.RIDEV_INPUTSINK;
            devices[0].hwndTarget = _hWnd;
            if (!RegisterRawInputDevices(devices, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                throw new WinApiException("Cannot register raw mouse input.");
            }

            devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = RawInputDeviceUsagePage.HID_USAGE_PAGE_GENERIC;
            devices[0].usUsage = RawInputDeviceUsage.HID_USAGE_GENERIC_KEYBOARD;
            devices[0].dwFlags = RawInputDeviceFlags.RIDEV_INPUTSINK;
            devices[0].hwndTarget = _hWnd;
            if (!RegisterRawInputDevices(devices, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                throw new WinApiException("Cannot register raw keyboard input.");
            }

            while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        private IntPtr WndProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WindowsMessage.WM_INPUT)
            {
                int size = Marshal.SizeOf(typeof(RAWINPUT));
                int headerSize = Marshal.SizeOf(typeof(RAWINPUTHEADER));
                if (GetRawInputData(lParam, RID_INPUT, out RAWINPUT data, ref size, headerSize) == -1)
                {
                    _logger?.LogWarning($"GetRawInputData error.");
                }
                else
                {
                    if (data.header.dwType == RawInputHeaderType.RIM_TYPEKEYBOARD)
                    {
                        ProcessKeyboardEvent(data.keyboard);
                    }

                    if (data.header.dwType == RawInputHeaderType.RIM_TYPEMOUSE)
                    {
                        ProcessMouseEvent(data.mouse);
                    }
                }
            }

            if (msg == WindowsMessage.WM_DESTROY)
            {
                PostQuitMessage(0);
                return IntPtr.Zero;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
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