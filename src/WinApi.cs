using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Zergatul.Obs.InputOverlay
{
    internal static class WinApi
    {
        public static readonly IntPtr InvalidHandle = new IntPtr(-1L);

        public delegate IntPtr WndProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            AccessMask dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            CreationDisposition dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandleW(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.U2)]
        public static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateWindowExW(
            uint dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            WindowStyles dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateWindowExW(
            uint dwExStyle,
            IntPtr lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            WindowStyles dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowsMessage uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterRawInputDevices([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] RAWINPUTDEVICE[] pRawInputDevices, int uiNumDevices, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetRawInputData(IntPtr hRawInput, GetRawInputDataCommand uiCommand, out RAWINPUT pData, ref int pcbSize, int cbSizeHeader);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetRawInputData(IntPtr hRawInput, GetRawInputDataCommand uiCommand, IntPtr pData, ref int pcbSize, int cbSizeHeader);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, WindowsMessage Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetRawInputDeviceList([In, Out] RAWINPUTDEVICELIST[] RawInputDeviceList, ref int puiNumDevices, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetRawInputDeviceInfoW(IntPtr hDevice, GetRawDeviceInfoCommand uiCommand, ref RID_DEVICE_INFO pData, ref int pcbSize);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetRawInputDeviceInfoW(IntPtr hDevice, GetRawDeviceInfoCommand uiCommand, StringBuilder pData, ref int pcbSize);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool HidD_GetProductString(IntPtr HidDeviceObject, StringBuilder Buffer, int BufferLength);

        #region Structures

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public int cbSize;
            public WindowClassStyle style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public RawInputDeviceUsagePage usUsagePage;
            public RawInputDeviceUsage usUsage;
            public RawInputDeviceFlags dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RAWINPUT
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;
            [FieldOffset(24)]
            public RAWMOUSE mouse;
            [FieldOffset(24)]
            public RAWKEYBOARD keyboard;
            [FieldOffset(24)]
            public RAWHID hid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public RawInputType dwType;
            public int dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RAWMOUSE
        {
            [FieldOffset(0)]
            public RawMouseFlags usFlags;
            [FieldOffset(4)]
            public RawMouseButtonsFlags usButtonFlags;
            [FieldOffset(6)]
            public ushort usButtonData;
            [FieldOffset(12)]
            public int lLastX;
            [FieldOffset(16)]
            public int lLastY;
            [FieldOffset(20)]
            public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public RawKeyboardFlags Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWHID
        {
            public int dwSizeHid;
            public int dwCount;
            //BYTE[1] bRawData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public RawInputType dwType;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RID_DEVICE_INFO
        {
            [FieldOffset(0)]
            public int cbSize;
            [FieldOffset(4)]
            public RawInputType dwType;
            [FieldOffset(8)]
            public RID_DEVICE_INFO_MOUSE mouse;
            [FieldOffset(8)]
            public RID_DEVICE_INFO_KEYBOARD keyboard;
            [FieldOffset(8)]
            public RID_DEVICE_INFO_HID hid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RID_DEVICE_INFO_MOUSE
        {
            public MouseDeviceProperties dwId;
            public int dwNumberOfButtons;
            public int dwSampleRate;
            public bool fHasHorizontalWheel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RID_DEVICE_INFO_KEYBOARD
        {
            public KeyboardDeviceProperty dwType;
            public int dwSubType;
            public int dwKeyboardMode;
            public int dwNumberOfFunctionKeys;
            public int dwNumberOfIndicators;
            public int dwNumberOfKeysTotal;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RID_DEVICE_INFO_HID
        {
            public int dwVendorId;
            public int dwProductId;
            public int dwVersionNumber;
            public RawInputDeviceUsagePage usUsagePage;
            public RawInputDeviceUsage usUsage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public WindowsMessage message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public POINT pt;
            public int lPrivate;
        }

        #endregion

        #region Enums

        [Flags]
        public enum WindowStyles : uint
        {
            WS_BORDER = 0x800000,
            WS_CAPTION = 0xC00000,
            WS_CHILD = 0x40000000,
            WS_CLIPCHILDREN = 0x2000000,
            WS_CLIPSIBLINGS = 0x4000000,
            WS_DISABLED = 0x8000000,
            WS_DLGFRAME = 0x400000,
            WS_GROUP = 0x20000,
            WS_HSCROLL = 0x100000,
            WS_MAXIMIZE = 0x1000000,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x20000,
            WS_OVERLAPPED = 0x0,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUP = 0x80000000U,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_SIZEFRAME = 0x40000,
            WS_SYSMENU = 0x80000,
            WS_TABSTOP = 0x10000,
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x200000
        }

        public enum WindowsMessage : uint
        {
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_ACTIVATE = 0x0006,
            WM_KILLFOCUS = 0x0008,
            WM_PAINT = 0x000F,
            WM_ACTIVATEAPP = 0x001C,
            WM_GETMINMAXINFO = 0x0024,
            WM_GETICON = 0x007F,
            WM_NCCREATE = 0x0081,
            WM_NCCALCSIZE = 0x0083,
            WM_NCACTIVATE = 0x0086,
            WM_SYNCPAINT = 0x0088,
            WM_INPUT_DEVICE_CHANGE = 0x00FE,
            WM_INPUT = 0x00FF,
            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_NOTIFY = 0x0282
        }

        [Flags]
        public enum RawInputDeviceFlags : uint
        {
            RIDEV_INPUTSINK = 0x00000100,
            RIDEV_DEVNOTIFY = 0x00002000
        }

        public enum GetRawInputDataCommand : uint
        {
            RID_HEADER = 0x10000005,
            RID_INPUT = 0x10000003
        }

        public enum GetRawDeviceInfoCommand : uint
        {
            RIDI_PREPARSEDDATA = 0x20000005,
            RIDI_DEVICENAME = 0x20000007,
            RIDI_DEVICEINFO = 0x2000000b
        }

        [Flags]
        public enum MouseDeviceProperties : int
        {
            MOUSE_HID_HARDWARE = 0x0080,
            WHEELMOUSE_HID_HARDWARE = 0x0100,
            HORIZONTAL_WHEEL_PRESENT = 0x8000
        }

        public enum KeyboardDeviceProperty : int
        {
            Enhanced = 0x04,
            Japanese = 0x07,
            Korean = 0x08,
            Unknown = 0x51
        }

        [Flags]
        public enum WindowClassStyle : uint
        {
            CS_BYTEALIGNCLIENT = 0x1000,
            CS_BYTEALIGNWINDOW = 0x2000,
            CS_DBLCLKS = 0x0008,
            CS_CLASSDC = 0x0040,
            CS_DROPSHADOW = 0x00020000,
            CS_GLOBALCLASS = 0x4000,
            CS_HREDRAW = 0x0002,
            CS_VREDRAW = 0x0001
        }

        public enum RawInputDeviceUsagePage : ushort
        {
            HID_USAGE_PAGE_GENERIC = 0x01,
            HID_USAGE_PAGE_GAME = 0x05,
            HID_USAGE_PAGE_LED = 0x08,
            HID_USAGE_PAGE_BUTTON = 0x09
        }

        public enum RawInputDeviceUsage : ushort
        {
            HID_USAGE_GENERIC_POINTER = 0x01,
            HID_USAGE_GENERIC_MOUSE = 0x02,
            HID_USAGE_GENERIC_JOYSTICK = 0x04,
            HID_USAGE_GENERIC_GAMEPAD = 0x05,
            HID_USAGE_GENERIC_KEYBOARD = 0x06,
            HID_USAGE_GENERIC_KEYPAD = 0x07,
            HID_USAGE_GENERIC_MULTI_AXIS_CONTROLLER = 0x08
        }

        public enum RawInputType : int
        {
            RIM_TYPEMOUSE = 0,
            RIM_TYPEKEYBOARD = 1,
            RIM_TYPEHID = 2
        }

        public enum RawMouseFlags : ushort
        {
            MOUSE_MOVE_RELATIVE = 0x00,
            MOUSE_MOVE_ABSOLUTE = 0x01,
            MOUSE_VIRTUAL_DESKTOP = 0x02,
            MOUSE_ATTRIBUTES_CHANGED = 0x04,
            MOUSE_MOVE_NOCOALESCE = 0x08
        }

        [Flags]
        public enum RawMouseButtonsFlags : ushort
        {
            RI_MOUSE_BUTTON_1_DOWN = 0x0001,
            RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001,
            RI_MOUSE_BUTTON_1_UP = 0x0002,
            RI_MOUSE_LEFT_BUTTON_UP = 0x0002,
            RI_MOUSE_BUTTON_2_DOWN = 0x0004,
            RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0004,
            RI_MOUSE_BUTTON_2_UP = 0x0008,
            RI_MOUSE_RIGHT_BUTTON_UP = 0x0008,
            RI_MOUSE_BUTTON_3_DOWN = 0x0010,
            RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010,
            RI_MOUSE_BUTTON_3_UP = 0x0020,
            RI_MOUSE_MIDDLE_BUTTON_UP = 0x0020,
            RI_MOUSE_BUTTON_4_DOWN = 0x0040,
            RI_MOUSE_BUTTON_4_UP = 0x0080,
            RI_MOUSE_BUTTON_5_DOWN = 0x0100,
            RI_MOUSE_BUTTON_5_UP = 0x0200,
            RI_MOUSE_WHEEL = 0x0400,
            RI_MOUSE_HWHEEL = 0x0800
        }

        public enum RawKeyboardFlags : ushort
        {
            RI_KEY_MAKE = 0,
            RI_KEY_BREAK = 1,
            RI_KEY_E0 = 2,
            RI_KEY_E1 = 4
        }

        public enum DeviceCap : int
        {
            LOGPIXELSX = 88,
            LOGPIXELSY = 90,
            VREFRESH = 116,
        }

        [Flags]
        public enum AccessMask : uint
        {
            Zero = 0,
            GENERIC_WRITE = 0x40000000,
            GENERIC_READ = 0x80000000
        }

        [Flags]
        public enum FileShare : uint
        {
            FILE_SHARE_READ = 0x00000001,
            FILE_SHARE_WRITE = 0x00000002,
            FILE_SHARE_DELETE = 0x00000004
        }

        public enum CreationDisposition : uint
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5
        }

        #endregion
    }
}