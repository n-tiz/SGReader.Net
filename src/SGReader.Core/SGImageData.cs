using System;
using System.IO;
using SGReader.Core.Helpers;

namespace SGReader.Core
{
    public class SGImageData
    {
        public SGImageData(BinaryReader reader, bool includeAlpha)
        {
            Offset = reader.ReadUInt32();
            Length = reader.ReadUInt32();
            UncompressedLength = reader.ReadUInt32();
            reader.Skip(4);
            InvertOffset = reader.ReadInt32();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            reader.Skip(6);
            NumberOfAnimationSprites = reader.ReadUInt16();
            NumberOfOrientations = reader.ReadUInt16();
            XOffset = reader.ReadInt16();
            YOffset = reader.ReadInt16();
            reader.Skip(10);
            IsAnimationReversible = Convert.ToBoolean(reader.ReadByte());
            reader.Skip(1);
            Type = reader.ReadByte();
            IsDataFullyCompressed = Convert.ToBoolean(reader.ReadByte());
            IsDataExternal = Convert.ToBoolean(reader.ReadByte());
            IsImagePartlyCompressed = Convert.ToBoolean(reader.ReadByte());
            reader.Skip(2);
            BitmapId = reader.ReadByte();
            reader.Skip(1);
            AnimationSpeedId = reader.ReadByte();
            reader.Skip(5); //Skip 7 bytes

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
        public int InvertOffset { get; }
        public short Width { get; }
        public short Height { get; }
        public ushort NumberOfAnimationSprites { get; }
        public ushort NumberOfOrientations { get; }
        public short XOffset { get; }
        public short YOffset { get; }
        public bool IsAnimationReversible { get; }
        public byte Type { get; }
        public bool IsDataFullyCompressed { get; }
        public bool IsDataExternal { get; }
        public bool IsImagePartlyCompressed { get; }
        public byte BitmapId { get; }
        public byte AnimationSpeedId { get; }
        public uint AlphaOffset { get; }
        public uint AlphaLength { get; }

    }
}