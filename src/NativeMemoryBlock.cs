using System;
using System.Runtime.InteropServices;

namespace Zergatul.Obs.InputOverlay
{
    public class NativeMemoryBlock : IDisposable
    {
        public IntPtr Pointer { get; private set; }
        public int Length { get; private set; }

        public NativeMemoryBlock(IntPtr source, int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Pointer = Marshal.AllocHGlobal(length);
            Length = length;

            WinApi.Kernel32.CopyMemory(Pointer, source, (uint)length);
        }

        public void Dispose()
        {
            if (Pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Pointer);
                Pointer = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
        }

        ~NativeMemoryBlock()
        {
            if (Pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Pointer);
            }
        }
    }
}