using System;

namespace Zergatul.Obs.InputOverlay
{
    public class HandleDisposable : IDisposable
    {
        public IntPtr Handle { get; private set; }

        public HandleDisposable(IntPtr handle)
        {
            Handle = handle;
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                WinApi.Kernel32.CloseHandle(Handle);
                Handle = IntPtr.Zero;
                GC.SuppressFinalize(this);
            }
        }

        ~HandleDisposable()
        {
            if (Handle != IntPtr.Zero)
            {
                WinApi.Kernel32.CloseHandle(Handle);
                Handle = IntPtr.Zero;
            }
        }
    }
}