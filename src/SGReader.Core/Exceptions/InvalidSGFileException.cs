using System;

namespace SGReader.Core.Exceptions
{
    internal class InvalidSGFileException : Exception
    {
        public SGFile File { get; }

        public InvalidSGFileException(string message, SGFile file) : base(message)
        {
            File = file;
        }

    }
}