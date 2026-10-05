using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FastBitmapLib;
using SGReader.Core.Exceptions;

namespace SGReader.Core
{
    public class SGImage : IDisposable
    {
        // C3 / Pharaoh / Zeus tiles
        private const int IsometricTileWidth = 58;
        private const int IsometricTileHeight = 30;
        private const int IsometricTileBytes = 1800;

        // Emperor tiles
        private const int IsometricLargeTileWidth = 78;
        private const int IsometricLargeTileHeight = 40;
        private const int IsometricLargeTileBytes = 3200;

        private const ushort Transparent555 = 0xf81f;
        private const int OpaqueBlackArgb = unchecked((int)0xff000000);
        private const int PureRedArgb = unchecked((int)0xffff0000);
        private const int ShadowArgb = unchecked((int)0x88000000);

        // External .555 records are stored one byte past the documented offset.
        private const int ExternalOffsetBias = 1;

        // Some C3 graphics truncate the last image by 4 bytes at EOF.
        private const int MissingTrailingBytesTolerance = 4;

        private readonly SGImageData _data;
        private SGImageData _workData;

        public int Id { get; }
        public string Description { get; }
        public string FullDescription { get; }
        public bool IsInverted { get; set; }
        public SGBitmap Parent { get; set; }
        public int InvertOffset => _data.InvertOffset;
        public int BitmapId => _workData?.BitmapId ?? _data.BitmapId;
        public int Width => _workData.Width;
        public int Height => _workData.Height;
        public byte Type => _workData.Type;
        public int AnimationSprites => _workData.NumberOfAnimationSprites;
        public int Orientations => _workData.NumberOfOrientations;
        public bool IsAnimationReversible => _workData.IsAnimationReversible;
        public byte AnimationSpeedId => _workData.AnimationSpeedId;

        // Mirror records may keep their own hotspot; otherwise mirror the source hotspot
        // after FlipX (Julius: draw at position - sprite_offset).
        public short XOffset
        {
            get
            {
                if (!IsInverted)
                    return _workData.XOffset;

                if (_data.XOffset != 0 || _data.YOffset != 0)
                    return _data.XOffset;

                return (short)Math.Max(0, _workData.Width - 1 - _workData.XOffset);
            }
        }

        public short YOffset
        {
            get
            {
                if (!IsInverted)
                    return _workData.YOffset;

                if (_data.XOffset != 0 || _data.YOffset != 0)
                    return _data.YOffset;

                return _workData.YOffset;
            }
        }

        public SGImage(int id, BinaryReader reader, bool includeAlpha)
        {
            Id = id;
            _workData = new SGImageData(reader, includeAlpha);
            _data = _workData;
            IsInverted = _data.InvertOffset != 0;
            Description = $"{_workData.Width}x{_workData.Height} ({_workData.BitmapId}-{_workData.NumberOfAnimationSprites})";
            FullDescription =
                $"ID {Id}: offset {_workData.Offset}, length {_workData.Length}, " +
                $"width {_workData.Width}, height {_workData.Height}, type {_workData.Type}, " +
                $"{(_workData.IsDataExternal ? "external" : "internal")}";
        }

        public void SetInvertedImage(SGImage image) => _workData = image._data;

        public void Dispose()
        {
            // Pixel buffers are created on demand and owned by the caller.
        }

        public Bitmap CreateImage()
        {
            if (Parent == null)
                throw new InvalidSGImageException("Image has no bitmap parent", this);

            if (_workData.Width <= 0 || _workData.Height <= 0 || _workData.Length <= 0)
                return null;

            byte[] buffer = ReadPixelBuffer();
            var result = new Bitmap(_workData.Width, _workData.Height, PixelFormat.Format32bppArgb);

            using (var fastBitmap = result.FastLock())
            {
                DecodePixels(fastBitmap, buffer);

                if (_workData.AlphaLength > 0)
                {
                    byte[] alphaBuffer = buffer.Skip((int)_workData.Length).ToArray();
                    ApplyAlphaMask(fastBitmap, alphaBuffer);
                }
            }

            if (IsInverted)
                result.RotateFlip(RotateFlipType.RotateNoneFlipX);

            return result;
        }

        private void DecodePixels(FastBitmap result, byte[] buffer)
        {
            if (_workData.Type == 30)
            {
                DecodeIsometric(result, buffer);
                return;
            }

            switch (_workData.Type)
            {
                case 0:
                case 1:
                case 10:
                case 12:
                case 13:
                case 20:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Type '{_workData.Type}' is not valid.");
            }

            // Encoding is independent from the gameplay type: fully-compressed => RLE.
            if (_workData.IsDataFullyCompressed)
                DecodeSpriteRle(result, buffer, _workData.Length);
            else
                DecodePlain(result, buffer);
        }

        private byte[] ReadPixelBuffer()
        {
            if (Parent == null)
                throw new InvalidSGImageException("Unable to open 555 file", this);

            lock (Parent.SyncRoot)
            {
                var file = Parent.OpenFile(_workData.IsDataExternal);
                if (file == null)
                    throw new InvalidSGImageException("Unable to open 555 file", this);

                int dataLength = (int)(_workData.Length + _workData.AlphaLength);
                if (dataLength <= 0)
                    throw new InvalidSGImageException($"Invalid data length: {dataLength}", this);

                byte[] buffer = new byte[dataLength];
                long offset = _workData.Offset - (_workData.IsDataExternal ? ExternalOffsetBias : 0);
                file.Seek(offset, SeekOrigin.Begin);

                int dataRead = file.Read(buffer, 0, dataLength);
                if (dataLength == dataRead)
                    return buffer;

                if (dataRead + MissingTrailingBytesTolerance == dataLength && file.Position == file.Length)
                {
                    buffer[dataRead] = buffer[dataRead + 1] = 0;
                    buffer[dataRead + 2] = buffer[dataRead + 3] = 0;
                    return buffer;
                }

                throw new InvalidSGImageException(
                    $"Unable to read {dataLength} bytes from file (read {dataRead} bytes)", this);
            }
        }

        private void DecodePlain(FastBitmap result, byte[] buffer)
        {
            if (_workData.Height * _workData.Width * 2 != (int)_workData.Length)
                throw new InvalidSGImageException("Image data length doesn't match image size", this);

            int i = 0;
            for (int y = 0; y < _workData.Height; y++)
            {
                for (int x = 0; x < _workData.Width; x++, i += 2)
                    Set555Pixel(result, x, y, (ushort)(buffer[i] | (buffer[i + 1] << 8)));
            }
        }

        private void DecodeIsometric(FastBitmap result, byte[] buffer)
        {
            WriteIsometricBase(result, buffer);
            int topOffset = (int)_workData.UncompressedLength;
            uint topLength = _workData.Length - _workData.UncompressedLength;
            DecodeSpriteRle(result, buffer.Skip(topOffset).ToArray(), topLength);
        }

        private void DecodeSpriteRle(FastBitmap result, byte[] buffer, uint length)
        {
            int i = 0;
            int x = 0;
            int y = 0;
            int width = result.Width;

            while (i < length)
            {
                byte c = buffer[i++];
                if (c == 255)
                {
                    x += buffer[i++];
                    while (x >= width)
                    {
                        y++;
                        x -= width;
                    }
                }
                else
                {
                    for (int j = 0; j < c; j++, i += 2)
                    {
                        Set555Pixel(result, x, y, (ushort)(buffer[i] | (buffer[i + 1] << 8)));
                        x++;
                        if (x >= width)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }
        }

        private void ApplyAlphaMask(FastBitmap result, byte[] alphaBuffer)
        {
            int i = 0;
            int x = 0;
            int y = 0;
            int width = result.Width;
            int length = (int)_workData.AlphaLength;

            while (i < length)
            {
                byte c = alphaBuffer[i++];
                if (c == 255)
                {
                    x += alphaBuffer[i++];
                    while (x >= width)
                    {
                        y++;
                        x -= width;
                    }
                }
                else
                {
                    for (int j = 0; j < c; j++, i++)
                    {
                        SetAlphaPixel(result, x, y, alphaBuffer[i]);
                        x++;
                        if (x >= width)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }
        }

        private void WriteIsometricBase(FastBitmap result, byte[] buffer)
        {
            int size = _workData.Flags[3];
            int width = result.Width;
            int height = (width + 2) / 2;
            int yOffset = result.Height - height;

            if (size == 0)
            {
                // Prefer regular tiles when both would divide the height evenly.
                if (height % IsometricTileHeight == 0)
                    size = height / IsometricTileHeight;
                else if (height % IsometricLargeTileHeight == 0)
                    size = height / IsometricLargeTileHeight;
            }

            if (size == 0)
                throw new InvalidSGImageException($"Unknown isometric tile size: height {height}", this);

            int tileBytes;
            int tileHeight;
            int tileWidth;
            if (IsometricTileHeight * size == height)
            {
                tileBytes = IsometricTileBytes;
                tileHeight = IsometricTileHeight;
                tileWidth = IsometricTileWidth;
            }
            else if (IsometricLargeTileHeight * size == height)
            {
                tileBytes = IsometricLargeTileBytes;
                tileHeight = IsometricLargeTileHeight;
                tileWidth = IsometricLargeTileWidth;
            }
            else
            {
                throw new InvalidSGImageException(
                    $"Unknown tile size: {2 * height / size} (height {height}, width {width}, size {size})",
                    this);
            }

            if ((width + 2) * height != (int)_workData.UncompressedLength)
            {
                throw new InvalidSGImageException(
                    $"Data length doesn't match footprint size: {(width + 2) * height} vs {_workData.UncompressedLength}",
                    this);
            }

            int tileIndex = 0;
            for (int y = 0; y < size + (size - 1); y++)
            {
                int xOffset = (y < size ? size - y - 1 : y - size + 1) * tileHeight;
                int tilesInRow = y < size ? y + 1 : 2 * size - y - 1;
                for (int x = 0; x < tilesInRow; x++, tileIndex++)
                {
                    WriteIsometricTile(result, buffer, tileIndex * tileBytes, xOffset, yOffset, tileWidth, tileHeight);
                    xOffset += tileWidth + 2;
                }

                yOffset += tileHeight / 2;
            }
        }

        private void WriteIsometricTile(
            FastBitmap result,
            byte[] buffer,
            int bufferOffset,
            int xOffset,
            int yOffset,
            int tileWidth,
            int tileHeight)
        {
            int halfHeight = tileHeight / 2;
            int i = bufferOffset;

            for (int y = 0; y < halfHeight; y++)
            {
                int start = tileHeight - 2 * (y + 1);
                int end = tileWidth - start;
                for (int x = start; x < end; x++, i += 2)
                {
                    Set555Pixel(result, xOffset + x, yOffset + y,
                        (ushort)((buffer[i + 1] << 8) | buffer[i]));
                }
            }

            for (int y = halfHeight; y < tileHeight; y++)
            {
                int start = 2 * y - tileHeight;
                int end = tileWidth - start;
                for (int x = start; x < end; x++, i += 2)
                {
                    Set555Pixel(result, xOffset + x, yOffset + y,
                        (ushort)((buffer[i + 1] << 8) | buffer[i]));
                }
            }
        }

        private static void Set555Pixel(FastBitmap result, int x, int y, ushort color)
        {
            if (color == Transparent555)
                return;

            int rgb = OpaqueBlackArgb;
            rgb |= ((color & 0x7c00) << 9) | ((color & 0x7000) << 4); // red
            rgb |= ((color & 0x3e0) << 6) | (color & 0x300);          // green
            rgb |= ((color & 0x1f) << 3) | ((color & 0x1c) >> 2);      // blue

            // Pure red becomes translucent black (game shadows).
            if (rgb == PureRedArgb)
                rgb = ShadowArgb;

            result.SetPixel(x, y, Color.FromArgb(rgb));
        }

        private static void SetAlphaPixel(FastBitmap result, int x, int y, byte color)
        {
            byte alpha = (byte)(((color & 0x1f) << 3) | ((color & 0x1c) >> 2));
            int rgb = result.GetPixel(x, y).ToArgb() & 0xffffff;
            result.SetPixel(x, y, Color.FromArgb((alpha << 24) | rgb));
        }
    }
}
