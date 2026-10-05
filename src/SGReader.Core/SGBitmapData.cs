using System;
using System.IO;
using System.Linq;
using SGReader.Core.Helpers;

namespace SGReader.Core
{
    public class SGBitmapData
    {
        public string FileName { get; }
        public string Comment { get; }

        public uint Width { get; }
        public uint Height { get; }
        public uint NumImages { get; }
        public uint StartIndex { get; }
        public uint EndIndex { get; }

        // Record is 200 bytes: 65 filename + 51 comment + 5×u32 + 64 unknown (Pecunia skipRawData(64)).
        public SGBitmapData(BinaryReader reader)
        {
            var filename = reader.ReadChars(65).Append('\0').ToArray();
            FileName = new string(filename, 0, Array.IndexOf(filename, '\0'));
            var comment = reader.ReadChars(51).Append('\0').ToArray();
            Comment = new string(comment, 0, Array.IndexOf(comment, '\0'));
            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            NumImages = reader.ReadUInt32();
            StartIndex = reader.ReadUInt32();
            EndIndex = reader.ReadUInt32();
            reader.Skip(64);
        }
    }
}
