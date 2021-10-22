using System;

namespace SGReader.Core
{
    internal class InvalidSGFileException : Exception
    {
        public SGFile File { get; }

        public InvalidSGFileException(string message, SGFile file)
        {
            File = file;
        }

    }
}