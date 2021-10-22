using System;
using System.IO;

namespace SGReader.Core
{
    public class SGHeader
    {
        public uint SGFileSize { get; }
        public uint Version { get; }
        public uint Unknown { get; }

        public int MaxImageRecords { get; }
        public int NumImageRecords { get; }
        public int NumBitmapRecords { get; }
        public int NumBitmapRecordsWithoutSystem { get; }

        public uint TotalFileSize { get; }
        public uint FileSize555 { get; }
        public uint FileSizeExternal { get; }

        public SGHeader(BinaryReader reader)
        {
            SGFileSize = reader.ReadUInt32();
            Version = reader.ReadUInt32();
            Unknown = reader.ReadUInt32();

            MaxImageRecords = reader.ReadInt32();
            NumImageRecords = reader.ReadInt32();
            NumBitmapRecords = reader.ReadInt32();
            NumBitmapRecordsWithoutSystem = reader.ReadInt32();

            TotalFileSize = reader.ReadUInt32();
            FileSize555 = reader.ReadUInt32();
            FileSizeExternal = reader.ReadUInt32();
        }
    }
}