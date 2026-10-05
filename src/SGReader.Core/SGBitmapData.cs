using System;
using System.IO;
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

        public SGBitmapData(BinaryReader reader)
        {
            FileName = reader.ReadFixedString(SGFormat.BitmapFileNameLength);
            Comment = reader.ReadFixedString(SGFormat.BitmapCommentLength);
            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            NumImages = reader.ReadUInt32();
            StartIndex = reader.ReadUInt32();
            EndIndex = reader.ReadUInt32();
            reader.Skip(SGFormat.BitmapTrailingUnknownBytes);
        }
    }
}
