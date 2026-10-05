using System;
using System.IO;

namespace SGReader.Core.Helpers
{
    public static class BinaryReaderExtensions
    {
        public static void Skip(this BinaryReader self, int bytes)
            => self.BaseStream.Seek(bytes, SeekOrigin.Current);

        /// <summary>Reads a fixed-width Latin-1 field and trims at the first null.</summary>
        public static string ReadFixedString(this BinaryReader self, int length)
        {
            var chars = self.ReadChars(length);
            int end = Array.IndexOf(chars, '\0');
            return new string(chars, 0, end < 0 ? chars.Length : end);
        }
    }
}
