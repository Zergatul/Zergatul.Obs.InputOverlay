using System;
using System.Buffers;

namespace Zergatul.Obs.InputOverlay
{
    public class StaticSizeArrayBufferWriter : IBufferWriter<byte>
    {
        private byte[] _buffer;
        private int _position;

        public StaticSizeArrayBufferWriter(byte[] buffer)
        {
            _buffer = buffer;
            _position = 0;
        }

        public void Advance(int count)
        {
            _position += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (_position == _buffer.Length)
            {
                throw new InvalidOperationException("No space left.");
            }

            if (sizeHint == 0)
            {
                return _buffer.AsMemory(_position, _buffer.Length - _position);
            }
            else
            {
                if (_position + sizeHint > _buffer.Length)
                {
                    throw new InvalidOperationException("Not enough space.");
                }

                return _buffer.AsMemory(_position, sizeHint);
            }
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (_position == _buffer.Length)
            {
                throw new InvalidOperationException("No space left.");
            }

            if (sizeHint == 0)
            {
                return _buffer.AsSpan(_position, _buffer.Length - _position);
            }
            else
            {
                if (_position + sizeHint > _buffer.Length)
                {
                    throw new InvalidOperationException("Not enough space.");
                }

                return _buffer.AsSpan(_position, sizeHint);
            }
        }

        public Memory<byte> GetWritten()
        {
            return _buffer.AsMemory(0, _position);
        }
    }
}