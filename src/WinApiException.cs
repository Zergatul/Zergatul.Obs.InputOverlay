using System;
using System.Runtime.InteropServices;

namespace Zergatul.Obs.InputOverlay
{
    public class WinApiException : Exception
    {
        public WinApiException()
            : this(null)
        {
            
        }

        public WinApiException(string message)
            : base(message)
        {
            HResult = Marshal.GetLastWin32Error();
        }
    }
}