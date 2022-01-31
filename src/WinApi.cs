using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Zergatul.Obs.InputOverlay
{
    internal static class WinApi
    {
        public static readonly IntPtr InvalidHandle = new IntPtr(-1L);

        #region Kernel32

        public static class Kernel32
        {
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

            [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
            public static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

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
        }

        #endregion

        #region User32

        public static class User32
        {
            public delegate IntPtr WndProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam);

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

            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int GetRawInputDeviceInfoW(IntPtr hDevice, GetRawDeviceInfoCommand uiCommand, IntPtr pData, ref int pcbSize);

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
                public uint dwSizeHid;
                public uint dwCount;
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
        }

        #endregion

        #region Hid

        public static class Hid
        {
            [DllImport("hid.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool HidD_GetProductString(
                [In] IntPtr HidDeviceObject,
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
                [In] int BufferLength);

            [DllImport("hid.dll", SetLastError = true)]
            public static extern HidPStatus HidP_GetCaps(
                [In] IntPtr PreparsedData,
                ref HIDP_CAPS Capabilities);

            [DllImport("hid.dll", SetLastError = true)]
            public static extern HidPStatus HidP_GetButtonCaps(
                [In] HIDP_REPORT_TYPE ReportType,
                [Out] HIDP_BUTTON_CAPS[] ButtonCaps,
                ref ushort ButtonCapsLength,
                [In] IntPtr PreparsedData);

            [DllImport("hid.dll", SetLastError = true)]
            public static extern HidPStatus HidP_GetValueCaps(
                [In] HIDP_REPORT_TYPE ReportType,
                [Out] HIDP_VALUE_CAPS[] ValueCaps,
                ref ushort ValueCapsLength,
                [In] IntPtr PreparsedData);

            [DllImport("hid.dll", SetLastError = true)]
            public static extern HidPStatus HidP_GetUsagesEx(
                [In] HIDP_REPORT_TYPE ReportType,
                [In] ushort LinkCollection,
                [In, Out] USAGE_AND_PAGE[] ButtonList,
                [In, Out] ref uint UsageLength,
                [In] IntPtr PreparsedData,
                [In] IntPtr Report,
                [In] uint ReportLength);

            [DllImport("hid.dll", SetLastError = true)]
            public static extern HidPStatus HidP_GetUsageValue(
                [In] HIDP_REPORT_TYPE ReportType,
                [In] User32.RawInputDeviceUsagePage UsagePage,
                [In] ushort LinkCollection,
                [In] ushort Usage,
                [Out] out uint UsageValue,
                [In] IntPtr PreparsedData,
                [In] IntPtr Report,
                [In] uint ReportLength);

            [DllImport("hid.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool HidD_GetSerialNumberString(
                [In] IntPtr HidDeviceObject,
                [Out] byte[] Buffer,
                [In] int BufferLength);

            [DllImport("hid.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool HidD_GetIndexedString(
                [In] IntPtr HidDeviceObject,
                [In] int StringIndex,
                [Out] byte[] Buffer,
                [In] int BufferLength);

            [DllImport("hid.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool HidD_GetManufacturerString(
              [In] IntPtr HidDeviceObject,
              [Out] byte[] Buffer,
              [In] int BufferLength);

            [DllImport("hid.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool HidD_GetFeature(
                [In] IntPtr HidDeviceObject,
                [Out] byte[] ReportBuffer,
                [In] int ReportBufferLength);

            [DllImport("hid.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool HidD_GetPhysicalDescriptor(
                [In] IntPtr HidDeviceObject,
                [Out] byte[] Buffer,
                [In] int BufferLength);

            [StructLayout(LayoutKind.Sequential)]
            public struct HIDP_CAPS
            {
                public User32.RawInputDeviceUsage Usage;
                public User32.RawInputDeviceUsagePage UsagePage;
                public ushort InputReportByteLength;
                public ushort OutputReportByteLength;
                public ushort FeatureReportByteLength;

                public ushort Reserved01;
                public ushort Reserved02;
                public ushort Reserved03;
                public ushort Reserved04;
                public ushort Reserved05;
                public ushort Reserved06;
                public ushort Reserved07;
                public ushort Reserved08;
                public ushort Reserved09;
                public ushort Reserved10;
                public ushort Reserved11;
                public ushort Reserved12;
                public ushort Reserved13;
                public ushort Reserved14;
                public ushort Reserved15;
                public ushort Reserved16;
                public ushort Reserved17;

                public ushort NumberLinkCollectionNodes;

                public ushort NumberInputButtonCaps;
                public ushort NumberInputValueCaps;
                public ushort NumberInputDataIndices;

                public ushort NumberOutputButtonCaps;
                public ushort NumberOutputValueCaps;
                public ushort NumberOutputDataIndices;

                public ushort NumberFeatureButtonCaps;
                public ushort NumberFeatureValueCaps;
                public ushort NumberFeatureDataIndices;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct HIDP_BUTTON_CAPS
            {
                [FieldOffset(0)]
                public User32.RawInputDeviceUsagePage UsagePage;
                [FieldOffset(2)]
                public byte ReportID;
                [FieldOffset(3)]
                public byte IsAlias;
                [FieldOffset(4)]
                public ushort BitField;
                [FieldOffset(6)]
                public ushort LinkCollection;
                [FieldOffset(8)]
                public User32.RawInputDeviceUsage LinkUsage;
                [FieldOffset(10)]
                public User32.RawInputDeviceUsagePage LinkUsagePage;
                [FieldOffset(12)]
                public byte IsRange;
                [FieldOffset(13)]
                public byte IsStringRange;
                [FieldOffset(14)]
                public byte IsDesignatorRange;
                [FieldOffset(15)]
                public byte IsAbsolute;
                [FieldOffset(16)]
                public uint Reserved01;
                [FieldOffset(20)]
                public uint Reserved02;
                [FieldOffset(24)]
                public uint Reserved03;
                [FieldOffset(28)]
                public uint Reserved04;
                [FieldOffset(32)]
                public uint Reserved05;
                [FieldOffset(36)]
                public uint Reserved06;
                [FieldOffset(40)]
                public uint Reserved07;
                [FieldOffset(44)]
                public uint Reserved08;
                [FieldOffset(48)]
                public uint Reserved09;
                [FieldOffset(52)]
                public uint Reserved10;
                [FieldOffset(56)]
                public _Range Range;
                [FieldOffset(56)]
                public _NotRange NotRange;

                [StructLayout(LayoutKind.Sequential)]
                public struct _Range
                {
                    public ushort UsageMin, UsageMax;
                    public ushort StringMin, StringMax;
                    public ushort DesignatorMin, DesignatorMax;
                    public ushort DataIndexMin, DataIndexMax;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct _NotRange
                {
                    public ushort Usage, Reserved1;
                    public ushort StringIndex, Reserved2;
                    public ushort DesignatorIndex, Reserved3;
                    public ushort DataIndex, Reserved4;
                }
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct HIDP_VALUE_CAPS
            {
                [FieldOffset(0)]
                public User32.RawInputDeviceUsagePage UsagePage;
                [FieldOffset(2)]
                public byte ReportID;
                [FieldOffset(3)]
                public byte IsAlias;
                [FieldOffset(4)]
                public ushort BitField;
                [FieldOffset(6)]
                public ushort LinkCollection;
                [FieldOffset(8)]
                public User32.RawInputDeviceUsage LinkUsage;
                [FieldOffset(10)]
                public User32.RawInputDeviceUsagePage LinkUsagePage;
                [FieldOffset(12)]
                public byte IsRange;
                [FieldOffset(13)]
                public byte IsStringRange;
                [FieldOffset(14)]
                public byte IsDesignatorRange;
                [FieldOffset(15)]
                public byte IsAbsolute;
                [FieldOffset(16)]
                public byte HasNull;
                [FieldOffset(17)]
                public byte Reserved;
                [FieldOffset(18)]
                public ushort BitSize;
                [FieldOffset(20)]
                public ushort ReportCount;
                [FieldOffset(22)]
                public ushort Reserved1;
                [FieldOffset(24)]
                public ushort Reserved2;
                [FieldOffset(26)]
                public ushort Reserved3;
                [FieldOffset(28)]
                public ushort Reserved4;
                [FieldOffset(30)]
                public ushort Reserved5;
                [FieldOffset(32)]
                public uint UnitsExp;
                [FieldOffset(36)]
                public uint Units;
                [FieldOffset(40)]
                public int LogicalMin;
                [FieldOffset(44)]
                public int LogicalMax;
                [FieldOffset(48)]
                public int PhysicalMin;
                [FieldOffset(52)]
                public int PhysicalMax;
                [FieldOffset(56)]
                public _Range Range;
                [FieldOffset(56)]
                public _NotRange NotRange;

                [StructLayout(LayoutKind.Sequential)]
                public struct _Range
                {
                    public ushort UsageMin, UsageMax;
                    public ushort StringMin, StringMax;
                    public ushort DesignatorMin, DesignatorMax;
                    public ushort DataIndexMin, DataIndexMax;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct _NotRange
                {
                    public ushort Usage, Reserved1;
                    public ushort StringIndex, Reserved2;
                    public ushort DesignatorIndex, Reserved3;
                    public ushort DataIndex, Reserved4;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct USAGE_AND_PAGE
            {
                public User32.RawInputDeviceUsage Usage;
                public User32.RawInputDeviceUsagePage UsagePage;
            }

            public enum HidPStatus : uint
            {
                HIDP_STATUS_SUCCESS = 0x00110000,
                HIDP_STATUS_INVALID_PREPARSED_DATA = 0xC0110001,
                HIDP_STATUS_INVALID_REPORT_TYPE = 0xc0110002,
                HIDP_STATUS_INVALID_REPORT_LENGTH = 0xc0110003,
                HIDP_STATUS_USAGE_NOT_FOUND = 0xC0110004,
                HIDP_STATUS_VALUE_OUT_OF_RANGE = 0xC0110005,
                HIDP_STATUS_BAD_LOG_PHY_VALUES = 0xC0110006,
                HIDP_STATUS_BUFFER_TOO_SMALL = 0xC0110007,
                HIDP_STATUS_INTERNAL_ERROR = 0xC0110008,
                HIDP_STATUS_I8042_TRANS_UNKNOWN = 0xC0110009,
                HIDP_STATUS_INCOMPATIBLE_REPORT_ID = 0xC011000A,
                HIDP_STATUS_NOT_VALUE_ARRAY = 0xC011000B,
                HIDP_STATUS_IS_VALUE_ARRAY = 0xC011000C,
                HIDP_STATUS_DATA_INDEX_NOT_FOUND = 0xC011000D,
                HIDP_STATUS_DATA_INDEX_OUT_OF_RANGE = 0xC011000E,
                HIDP_STATUS_BUTTON_NOT_PRESSED = 0xC011000F,
                HIDP_STATUS_REPORT_DOES_NOT_EXIST = 0xC0110010,
                HIDP_STATUS_NOT_IMPLEMENTED = 0xC0110020,
                HIDP_STATUS_I8242_TRANS_UNKNOWN = 0xC0110009,
            }

            public enum HIDP_REPORT_TYPE : int
            {
                HidP_Input,
                HidP_Output,
                HidP_Feature
            }
        }

        #endregion

        #region XInput

        public static class XInput
        {
            public const int XUSER_MAX_COUNT = 4;

            [DllImport("xinput1_4.dll")]
            public static extern Win32Error XInputGetState([In] int dwUserIndex, [Out] out XINPUT_STATE pState);

            [StructLayout(LayoutKind.Sequential)]
            public struct XINPUT_STATE
            {
                public uint dwPacketNumber;
                public XINPUT_GAMEPAD Gamepad;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct XINPUT_GAMEPAD
            {
                public ushort wButtons;
                public byte bLeftTrigger;
                public byte bRightTrigger;
                public short sThumbLX;
                public short sThumbLY;
                public short sThumbRX;
                public short sThumbRY;

                public bool Equals(XINPUT_GAMEPAD other)
                {
                    return
                        other.wButtons == wButtons &&
                        other.bLeftTrigger == bLeftTrigger &&
                        other.bRightTrigger == bRightTrigger &&
                        other.sThumbLX == sThumbLX &&
                        other.sThumbLY == sThumbLY &&
                        other.sThumbRX == sThumbRX &&
                        other.sThumbRY == sThumbRY;
                }
            }
        }

        #endregion

        #region Enums

        public enum Win32Error : uint
        {
            ERROR_SUCCESS = 0x0000,
            ERROR_INVALID_FUNCTION = 0x0001,
            ERROR_INVALID_HANDLE = 0x0006,
            ERROR_GEN_FAILURE = 0x001F,
            ERROR_INVALID_PARAMETER = 0x0057,
            ERROR_NOACCESS = 0x03E6
        }

        #endregion
    }
}