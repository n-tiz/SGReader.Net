using System;
using System.IO;

namespace SGReader.Core
{
    public enum SGFileVersion
    {
        SG2FormatDemo = 207, //C3 demo
        SG2Format = 211,
        SG3Format = 213,
        SG3FormatWithAlphaMask = 214
    }

    public class SGHeader
    {
        public const int HeaderSize = 80;

        public uint SGFileSize { get; }
        public SGFileVersion Version { get; }
        public uint Unknown { get; }

        public int MaxImageDataCount { get; }
        public int ImageDataCount { get; }
        public int BitmapDataCount { get; }
        public int BitmapDataCountWithoutSystem { get; }

        public uint TotalFileSize { get; }
        public uint FileSize555 { get; }
        public uint FileSizeExternal { get; }

        public SGHeader(BinaryReader reader)
        {
            SGFileSize = reader.ReadUInt32();
            Version = (SGFileVersion) reader.ReadUInt32();
            Unknown = reader.ReadUInt32();

            MaxImageDataCount = reader.ReadInt32();
            ImageDataCount = reader.ReadInt32();
            BitmapDataCount = reader.ReadInt32();
            BitmapDataCountWithoutSystem = reader.ReadInt32();

            TotalFileSize = reader.ReadUInt32();
            FileSize555 = reader.ReadUInt32();
            FileSizeExternal = reader.ReadUInt32();
        }
    }
}