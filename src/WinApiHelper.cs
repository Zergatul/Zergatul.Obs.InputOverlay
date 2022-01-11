using System;

namespace Zergatul.Obs.InputOverlay
{
    public static class WinApiHelper
    {
        public static string FormatWin32Error(int errorCode)
        {
            return $"(hex={FormatErrorCode(errorCode)} dec={errorCode})";
        }

        private static string FormatErrorCode(int code)
        {
            return "0x" + code.ToString("x2").PadLeft(8, '0');
        }

        private static string FormatIntPtr(IntPtr ptr)
        {
            return "0x" + ptr.ToInt64().ToString("x2").PadLeft(16, '0');
        }
    }
}