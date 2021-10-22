using System;
using System.IO;
using System.Linq;

namespace SGReader.Core
{
    public class SGBitmapData
    {
        char[] _unknown;

        public string FileName { get; }
        public string Comment { get; }

        public uint Width { get; }
        public uint Height { get; }
        public uint NumImages { get; }
        public uint StartIndex { get; }
        public uint EndIndex { get; }

        /* 4 bytes - uint between start & end */
        /* 16b, 4x int with unknown purpose */
        /*  8b, 2x int with (real?) width & height */
        /* 12b, 3x int: if any is non-zero: internal image */
        /* 24 more misc bytes, most zero */

        public SGBitmapData(BinaryReader reader)
        {
            var filename = reader.ReadChars(65).Append('\0').ToArray();
            FileName = new string(filename,0, Array.IndexOf(filename, '\0'));
            var comment = reader.ReadChars(51).Append('\0').ToArray();
            Comment = new string(comment, 0, Array.IndexOf(comment, '\0'));
            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            NumImages = reader.ReadUInt32();
            StartIndex = reader.ReadUInt32();
            EndIndex = reader.ReadUInt32();
            _unknown = reader.ReadChars(64).Append('\0').ToArray();
        }
    }
}