using System;

namespace SGReader.Core.Exceptions
{
    public class InvalidSGImageException : Exception
    {
        public SGImage SGImage { get; }

        public InvalidSGImageException(string message, SGImage sgImage) : base(message)
        {
            SGImage = sgImage;
        }
    }
}