using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace Zergatul.Obs.InputOverlay
{
    using static WinApi;

    public class InputHook : IInputHook
    {
        public event EventHandler<ButtonEvent> ButtonAction;

        private ILogger _logger;
        private readonly object _syncObject = new object();
        private readonly Queue<ButtonEvent> _buttonEventsQueue = new Queue<ButtonEvent>();
        private readonly BitArray _state = new BitArray(256);
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly Thread _thread;
        private volatile bool _shutdown = false;

        private WindowsHook _keyboardHook;
        private WindowsHook _mouseHook;

        public InputHook(ILogger<InputHook> logger)
        {
            _logger = logger;

            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    _logger?.LogWarning("App is running under non-admin. Hooks will not work from applications running as admin.");
            }

            _keyboardHook = new WindowsHook(WH_KEYBOARD_LL, KeyboardHookCallback, _logger);
            _mouseHook = new WindowsHook(WH_MOUSE_LL, MouseHookCallback, _logger);

            _thread = new Thread(DequeueThread);
            _thread.Start();
        }

        public void Dispose()
        {
            _shutdown = true;
            Thread.MemoryBarrier();
            _resetEvent.Set();
            _thread.Join();

            _keyboardHook?.Dispose();
            _mouseHook?.Dispose();

            _keyboardHook = null;
            _mouseHook = null;
        }

        private void DequeueThread()
        {
            while (!_shutdown)
            {
                _resetEvent.WaitOne();
                if (_shutdown)
                    break;

                while (true)
                {
                    ButtonEvent buttonEvent;
                    lock (_syncObject)
                    {
                        if (_buttonEventsQueue.Count == 0)
                            break;

                        buttonEvent = _buttonEventsQueue.Dequeue();
                    }

                    ButtonAction?.Invoke(this, buttonEvent);
                }
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                switch ((int)wParam)
                {
                    case WM_KEYDOWN:
                    case WM_KEYUP:
                        int vkCode = Marshal.ReadInt32(lParam);
                        Button button = GetButtonFromVKCode(vkCode);
                        if (button != Button.None)
                        {
                            if (wParam == (IntPtr)WM_KEYDOWN)
                                ProcessKeyDown(button);
                            if (wParam == (IntPtr)WM_KEYUP)
                                ProcessKeyUp(button);
                        }
                        break;

                    case WM_SYSKEYDOWN:
                        ProcessKeyDown(Button.Alt);
                        break;

                    case WM_SYSKEYUP:
                        ProcessKeyUp(Button.Alt);
                        break;
                }
            }
            return CallNextHookEx(_keyboardHook.HookHandle, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                Button button = Button.None;
                bool pressed = false;
                switch ((int)wParam)
                {
                    case WM_LBUTTONDOWN:
                        button = Button.Mouse1;
                        pressed = true;
                        break;

                    case WM_LBUTTONUP:
                        button = Button.Mouse1;
                        pressed = false;
                        break;

                    case WM_RBUTTONDOWN:
                        button = Button.Mouse2;
                        pressed = true;
                        break;

                    case WM_RBUTTONUP:
                        button = Button.Mouse2;
                        pressed = false;
                        break;

                    case WM_MBUTTONDOWN:
                        button = Button.Mouse3;
                        pressed = true;
                        break;

                    case WM_MBUTTONUP:
                        button = Button.Mouse3;
                        pressed = false;
                        break;

                    case WM_XBUTTONDOWN:
                        int mouseData = Marshal.ReadInt32(lParam + 8);
                        switch (mouseData >> 16)
                        {
                            case XBUTTON1: button = Button.Mouse4; break;
                            case XBUTTON2: button = Button.Mouse5; break;
                        }
                        pressed = true;
                        break;

                    case WM_XBUTTONUP:
                        mouseData = Marshal.ReadInt32(lParam + 8);
                        switch (mouseData >> 16)
                        {
                            case XBUTTON1: button = Button.Mouse4; break;
                            case XBUTTON2: button = Button.Mouse5; break;
                        }
                        pressed = false;
                        break;
                }

                if (button != Button.None)
                {
                    lock (_syncObject)
                    {
                        _buttonEventsQueue.Enqueue(new ButtonEvent(button, pressed));
                        _resetEvent.Set();
                    }
                }
            }

            return CallNextHookEx(_mouseHook.HookHandle, nCode, wParam, lParam);
        }

        private void ProcessKeyDown(Button button)
        {
            lock (_syncObject)
            {
                if (!_state[(int)button])
                {
                    _buttonEventsQueue.Enqueue(new ButtonEvent(button, true));
                    _state[(int)button] = true;
                    _resetEvent.Set();
                }
            }
        }

        private void ProcessKeyUp(Button button)
        {
            lock (_syncObject)
            {
                _buttonEventsQueue.Enqueue(new ButtonEvent(button, false));
                _state[(int)button] = false;
                _resetEvent.Set();
            }
        }

        private Button GetButtonFromVKCode(int vkCode)
        {
            switch (vkCode)
            {
                case VK_LBUTTON: return Button.Mouse1;
                case VK_RBUTTON: return Button.Mouse2;
                case VK_MBUTTON: return Button.Mouse3;
                case VK_XBUTTON1: return Button.Mouse4;
                case VK_XBUTTON2: return Button.Mouse5;

                case VK_TAB: return Button.Tab;
                case VK_SHIFT: return Button.Shift;
                case VK_CONTROL: return Button.Ctrl;
                case VK_MENU: return Button.Alt;
                case VK_CAPITAL: return Button.Caps;
                case VK_ESCAPE: return Button.Esc;
                case VK_SPACE: return Button.Space;
                case VK_RETURN: return Button.Enter;

                case VK_0: return Button.Num0;
                case VK_1: return Button.Num1;
                case VK_2: return Button.Num2;
                case VK_3: return Button.Num3;
                case VK_4: return Button.Num4;
                case VK_5: return Button.Num5;
                case VK_6: return Button.Num6;
                case VK_7: return Button.Num7;
                case VK_8: return Button.Num8;
                case VK_9: return Button.Num9;

                case VK_A: return Button.KeyA;
                case VK_B: return Button.KeyB;
                case VK_C: return Button.KeyC;
                case VK_D: return Button.KeyD;
                case VK_E: return Button.KeyE;
                case VK_F: return Button.KeyF;
                case VK_G: return Button.KeyG;
                case VK_H: return Button.KeyH;
                case VK_I: return Button.KeyI;
                case VK_J: return Button.KeyJ;
                case VK_K: return Button.KeyK;
                case VK_L: return Button.KeyL;
                case VK_M: return Button.KeyM;
                case VK_N: return Button.KeyN;
                case VK_O: return Button.KeyO;
                case VK_P: return Button.KeyP;
                case VK_Q: return Button.KeyQ;
                case VK_R: return Button.KeyR;
                case VK_S: return Button.KeyS;
                case VK_T: return Button.KeyT;
                case VK_U: return Button.KeyU;
                case VK_V: return Button.KeyV;
                case VK_W: return Button.KeyW;
                case VK_X: return Button.KeyX;
                case VK_Y: return Button.KeyY;
                case VK_Z: return Button.KeyZ;

                case VK_F1: return Button.F1;
                case VK_F2: return Button.F2;
                case VK_F3: return Button.F3;
                case VK_F4: return Button.F4;
                case VK_F5: return Button.F5;
                case VK_F6: return Button.F6;
                case VK_F7: return Button.F7;
                case VK_F8: return Button.F8;
                case VK_F9: return Button.F9;
                case VK_F10: return Button.F10;
                case VK_F11: return Button.F11;
                case VK_F12: return Button.F12;

                case VK_LSHIFT: return Button.Shift;
                case VK_RSHIFT: return Button.Shift;
                case VK_LCONTROL: return Button.Ctrl;
                case VK_RCONTROL: return Button.Ctrl;
                case VK_LMENU: return Button.Alt;
                case VK_RMENU: return Button.Alt;
                case VK_OEM_2: return Button.Slash;
                case VK_OEM_3: return Button.Tilde;
                case VK_OEM_5: return Button.Backslash;

                default:
                    //_logger.LogInformation($"New key {vkCode}");
                    return Button.None;
            }
        }
    }

    public struct ButtonEvent
    {
        public Button Button { get; }
        public bool Pressed { get; }

        public ButtonEvent(Button button, bool pressed)
        {
            Button = button;
            Pressed = pressed;
        }
    }
}