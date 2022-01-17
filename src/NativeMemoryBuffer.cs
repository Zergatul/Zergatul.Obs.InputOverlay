using System;
using System.Runtime.InteropServices;

namespace Zergatul.Obs.InputOverlay
{
    internal class NativeMemoryBuffer : IDisposable
    {
        public IntPtr Pointer { get; private set; }

        private int _capacity;

        public NativeMemoryBuffer()
            : this(1024)
        {

        }

        public NativeMemoryBuffer(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _capacity = capacity;
            Pointer = Marshal.AllocHGlobal(capacity);
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (capacity > _capacity)
            {
                Marshal.FreeHGlobal(Pointer);
                _capacity = capacity;
                Pointer = Marshal.AllocHGlobal(capacity);
            }
        }

        public NativeMemoryBlock ToMemoryBlock(int length)
        {
            return new NativeMemoryBlock(Pointer, length);
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

        ~NativeMemoryBuffer()
        {
            if (Pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Pointer);
            }
        }
    }
}