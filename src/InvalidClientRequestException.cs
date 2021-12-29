using System;

namespace Zergatul.Obs.InputOverlay
{
    public class InvalidClientRequestException : Exception
    {
        public InvalidClientRequestException()
            : base()
        {

        }

        public InvalidClientRequestException(string message)
            : base(message)
        {

        }
    }
}