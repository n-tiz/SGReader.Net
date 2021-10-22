using System;
using System.IO;

namespace SGReader.Core
{
    public class SGImageData
    {
        public SGImageData(BinaryReader reader, bool includeAlpha)
        {
            Offset = reader.ReadUInt32();
            Length = reader.ReadUInt32();
            UncompressedLength = reader.ReadUInt32();
            reader.BaseStream.Seek(4, SeekOrigin.Current); //Skip 4 bytes
            InvertOffset = reader.ReadInt32();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            reader.BaseStream.Seek(26, SeekOrigin.Current); //Skip 26 bytes
            Type = reader.ReadUInt16();
            Flags = Array.ConvertAll(reader.ReadBytes(4), b => (sbyte) b);
            BitmapId = reader.ReadByte();
            reader.BaseStream.Seek(7, SeekOrigin.Current); //Skip 7 bytes

            if (includeAlpha)
            {
                AlphaOffset = reader.ReadUInt32();
                AlphaLength = reader.ReadUInt32();
            }
            else
            {
                AlphaOffset = 0;
                AlphaLength = 0;
            }
        }

        public uint Offset { get; }
        public uint Length { get; }
        public uint UncompressedLength { get; }
        /* 4 zero bytes: */
        public int InvertOffset { get; }
        public short Width { get; }
        public short Height { get; }
        /* 26 unknown bytes, mostly zero, first four are 2 shorts */
        public ushort Type { get; }
        /* 4 flag/option-like bytes: */
        public sbyte[] Flags { get; }
        public byte BitmapId { get; }
        /* 3 bytes + 4 zero bytes */
        /* For D6 and up SG3 versions: alpha masks */
        public uint AlphaOffset { get; }
        public uint AlphaLength { get; }
    }
}