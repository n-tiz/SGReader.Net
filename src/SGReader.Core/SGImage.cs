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
        private const int IsometricTileWidth = 58;
        private const int IsometricTileHeight = 30;
        private const int IsometricTileBytes = 1800;
        private const int IsometricLargeTileWidth = 78;
        private const int IsometricLargeTileHeight = 40;
        private const int IsometricLargeTileBytes = 3200;

        private readonly SGImageData _data;
        private SGImageData _workData;

        public int Id { get; }
        public string Description { get; }
        public string FullDescription { get; }
        public bool IsInverted { get; set; }
        public SGBitmap Parent { get; set; } = null;
        public int InvertOffset => _data.InvertOffset;
        public int BitmapId => _workData?.BitmapId ?? _data.BitmapId;
        public int Width => _workData.Width;
        public int Height => _workData.Height;
        public int AnimationSprites => _workData.NumberOfAnimationSprites;
        public int Orientations => _workData.NumberOfOrientations;

        public SGImage(int id, BinaryReader reader, bool includeAlpha)
        {
            Id = id;
            _workData = new SGImageData(reader, includeAlpha);
            _data = _workData;
            IsInverted = _data.InvertOffset != 0;
            Description = $"{_workData.Width}x{_workData.Height} ({_workData.BitmapId}-{_workData.NumberOfAnimationSprites})";
            FullDescription = $"ID {Id}: offset {_workData.Offset}, length {_workData.Length}, width {_workData.Width}, height {_workData.Height}, type {_workData.Type}, {(_workData.IsDataExternal ? "external" : "internal")}";
        }

        public void SetInvertedImage(SGImage image)
        {
            _workData = image._data;
        }


        public void Dispose()
        {
            // _workData is deleted by whoever owns it
        }

        public Bitmap CreateImage()
        {
            // Trivial checks
            if (Parent == null)
            {
                throw new InvalidSGImageException("Image has no bitmap parent", this);
            }
            if (_workData.Width <= 0 || _workData.Height <= 0 || _workData.Length <= 0)
            {
                return null;
            }

            byte[] buffer = FillBuffer();
            if (buffer == null)
            {
                throw new InvalidSGImageException("Unable to load buffer", this);
            }

            Bitmap result = new Bitmap(_workData.Width, _workData.Height, PixelFormat.Format32bppArgb);
            using (var fastBitmap = result.FastLock())
            {
                switch (_workData.Type)
                {
                    case 0:
                        LoadSpriteImage(fastBitmap, buffer);
                        break;
                    case 1:
                    case 10:
                    case 12:
                    case 13:
                    case 20:
                        LoadPlainImage(fastBitmap, buffer);
                        break;
                    case 30:
                        LoadIsometricImage(fastBitmap, buffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Type '{_workData.Type}' is not valid.");
                }

                if (_workData.AlphaLength > 0)
                {
                    byte[] alphaBuffer = buffer.Skip((int) _workData.Length).ToArray();
                    LoadAlphaMask(fastBitmap, alphaBuffer);
                }
            }
            if (IsInverted)
            {
                result.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            return result;
        }

        private byte[] FillBuffer()
        {
            var file = Parent.OpenFile(_workData.IsDataExternal);
            if (file == null)
            {
                throw new InvalidSGImageException("Unable to open 555 file", this);
            }

            int dataLength = (int) (_workData.Length + _workData.AlphaLength);
            if (dataLength <= 0)
            {
                throw new InvalidSGImageException($"Invalid data length: {dataLength}", this); // not an error per se ?
            }
            byte[] buffer = new byte[dataLength];

            // Somehow externals have 1 byte added to their offset
            file.Seek(_workData.Offset - (_workData.IsDataExternal ? 1 : 0), SeekOrigin.Begin);

            int dataRead = file.Read(buffer, 0, dataLength);
            if (dataLength != dataRead)
            {
                if (dataRead + 4 == dataLength && file.Position == file.Length)
                {
                    // Exception for some C3 graphics: last image is 'missing' 4 bytes
                    buffer[dataRead] = buffer[dataRead + 1] = 0;
                    buffer[dataRead + 2] = buffer[dataRead + 3] = 0;
                }
                else
                    throw new InvalidSGImageException($"Unable to read {dataLength} bytes from file (read {dataRead} bytes)", this);
            }
            return buffer;
        }

        private void LoadPlainImage(FastBitmap result, byte[] buffer)
        {
            // Check whether the image data is OK
            if (_workData.Height * _workData.Width * 2 != (int)_workData.Length)
                throw new InvalidSGImageException("Image data length doesn't match image size", this);

            int i = 0;
            for (int y = 0; y < (int)_workData.Height; y++)
            {
                for (int x = 0; x < (int)_workData.Width; x++, i += 2)
                {
                    Set555Pixel(result, x, y, (ushort) (buffer[i] | (buffer[i + 1] << 8)));
                }
            }
        }

        private void LoadIsometricImage(FastBitmap result, byte[] buffer)
        {
            WriteIsometricBase(result, buffer);
            WriteTransparentImage(result, buffer.Skip((int) _workData.UncompressedLength).ToArray(), _workData.Length - _workData.UncompressedLength);
        }

        private void LoadSpriteImage(FastBitmap result, byte[] buffer)
        {
            WriteTransparentImage(result, buffer, _workData.Length);
        }
        
        private void LoadAlphaMask(FastBitmap result, byte[] alphaBuffer)
        {
            int i = 0;
            int x = 0, y = 0, j;
            int width = result.Width;
            int length = (int) _workData.AlphaLength;

            while (i < length)
            {
                byte c = alphaBuffer[i++];
                if (c == 255)
                {
                    /* The next byte is the number of pixels to skip */
                    x += alphaBuffer[i++];
                    while (x >= width)
                    {
                        y++; x -= width;
                    }
                }
                else
                {
                    /* `c' is the number of image data bytes */
                    for (j = 0; j < c; j++, i++)
                    {
                        SetAlphaPixel(result, x, y, alphaBuffer[i]);
                        x++;
                        if (x >= width)
                        {
                            y++; x = 0;
                        }
                    }
                }
            }
        }

        private void WriteIsometricBase(FastBitmap result, byte[] buffer)
        {
            throw new NotImplementedException();
            //int size = _workData.Flags[3];
            //int tileBytes, tileHeight, tileWidth;

            //int width = result.Width;
            //var height = (width + 2) / 2;
            //var heightOffset = result.Height - height;
            //var yOffset = heightOffset;

            //if (size == 0)
            //{
            //    /* Derive the tile size from the height (more regular than width) */
            //    /* Note that this causes a problem with 4x4 regular vs 3x3 large: */
            //    /* 4 * 30 = 120; 3 * 40 = 120 -- give precedence to regular */
            //    if (height % IsometricTileHeight == 0)
            //    {
            //        size = height / IsometricTileHeight;
            //    }
            //    else if (height % IsometricLargeTileHeight == 0)
            //    {
            //        size = height / IsometricLargeTileHeight;
            //    }
            //}
            //if (size == 0)
            //{
            //    throw new InvalidSGImageException($"Unknown isometric tile size: height {height}", this);
            //}

            ///* Determine whether we should use the regular or large (emperor) tiles */
            //if (IsometricTileHeight * size == height)
            //{
            //    /* Regular tile */
            //    tileBytes = IsometricTileBytes;
            //    tileHeight = IsometricTileHeight;
            //    tileWidth = IsometricTileWidth;
            //}
            //else if (IsometricLargeTileHeight * size == height)
            //{
            //    /* Large (emperor) tile */
            //    tileBytes = IsometricLargeTileBytes;
            //    tileHeight = IsometricLargeTileHeight;
            //    tileWidth = IsometricLargeTileWidth;
            //}
            //else
            //    throw new InvalidSGImageException(
            //        $"Unknown tile size: {2 * height / size} (height {height}, width {width}, size {size})", this);

            ///* Check if buffer length is enough: (width + 2) * height / 2 * 2bpp */
            //if ((width + 2) * height != (int)_workData.UncompressedLength)
            //    throw new InvalidSGImageException(
            //        $"Data length doesn't match footprint size: {(width + 2) * height} vs {_workData.UncompressedLength} ({_workData.Length}) {_workData.InvertOffset}",
            //        this);

            //int  i = 0;
            //for (int y = 0; y < (size + (size - 1)); y++)
            //{
            //    var xOffset = (y < size ? (size - y - 1) : (y - size + 1)) * tileHeight;
            //    int x;
            //    for (x = 0; x < (y < size ? y + 1 : 2 * size - y - 1); x++, i++)
            //    {
            //        WriteIsometricTile(result, buffer.Skip(i * tileBytes).ToArray(),
            //            xOffset, yOffset, tileWidth, tileHeight);
            //        xOffset += tileWidth + 2;
            //    }
            //    yOffset += tileHeight / 2;
            //}
        }

        private void WriteIsometricTile(FastBitmap result, byte[] buffer, int xOffset, int yOffset, int tileWidth, int tileHeight)
        {
            int halfHeight = tileHeight / 2;
            int x, y, i = 0;

            for (y = 0; y < halfHeight; y++)
            {
                int start = tileHeight - 2 * (y + 1);
                int end = tileWidth - start;
                for (x = start; x < end; x++, i += 2)
                {
                    Set555Pixel(result, xOffset + x, yOffset + y,
                        (ushort) ((buffer[i + 1] << 8) | buffer[i]));
                }
            }
            for (y = halfHeight; y < tileHeight; y++)
            {
                int start = 2 * y - tileHeight;
                int end = tileWidth - start;
                for (x = start; x < end; x++, i += 2)
                {
                    Set555Pixel(result, xOffset + x, yOffset + y,
                        (ushort) ((buffer[i + 1] << 8) | buffer[i]));
                }
            }
        }

        private void WriteTransparentImage(FastBitmap result, byte[] buffer, uint length)
        {
            int i = 0;
            int x = 0, y = 0, j;
            int width = result.Width;

            while (i < length)
            {
                byte c = buffer[i++];
                if (c == 255)
                {
                    /* The next byte is the number of pixels to skip */
                    x += buffer[i++];
                    while (x >= width)
                    {
                        y++; x -= width;
                    }
                }
                else
                {
                    /* `c' is the number of image data bytes */
                    for (j = 0; j < c; j++, i += 2)
                    {
                        Set555Pixel(result, x, y,  (ushort) (buffer[i] | (buffer[i + 1] << 8)));
                        x++;
                        if (x >= width)
                        {
                            y++; x = 0;
                        }
                    }
                }
            }
        }

        private void Set555Pixel(FastBitmap result, int x, int y, ushort color)
        {
            if (color == 0xf81f)
            {
                return;
            }

            int rgb = unchecked((int) 0xff000000);

            // Red: bits 11-15, should go to bits 17-24
            rgb |= ((color & 0x7c00) << 9) | ((color & 0x7000) << 4);

            // Green: bits 6-10, should go to bits 9-16
            rgb |= ((color & 0x3e0) << 6) | ((color & 0x300));

            // Blue: bits 1-5, should go to bits 1-8
            rgb |= ((color & 0x1f) << 3) | ((color & 0x1c) >> 2);

            // Transform red to transparent black (for shadows).
            if (rgb == unchecked((int)0xffff0000))
            {
                rgb = unchecked((int)0x88000000);
            }

            result.SetPixel(x, y, Color.FromArgb(rgb));
        }

        private void SetAlphaPixel(FastBitmap result, int x, int y, byte color)
        {
            /* Only the first five bits of the alpha channel are used */
            byte alpha = (byte) (((color & 0x1f) << 3) | ((color & 0x1c) >> 2));

            result.SetPixel(x, y, Color.FromArgb((alpha << 24) | (result.GetPixel(x, y).ToArgb() & 0xffffff)));
        }



    }

    //public class FastBitmap : IDisposable
    //{
    //    private readonly Bitmap _bitmap;
    //    private BitmapData _data;

    //    public int Width { get; }

    //    public int Height { get; }

    //    public FastBitmap(Bitmap bitmap)
    //    {
    //        _bitmap = bitmap;
    //        Width = bitmap.Width;
    //        Height = bitmap.Height;

    //        _data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
    //    }

    //    public void SetPixel(int x, int y, Color color)
    //    {
    //        int stride = _data.Stride;
    //        unsafe
    //        {
    //            byte* ptr = (byte*)_data.Scan0;
    //            ptr[(x * 3) + y * stride] = color.B;
    //            ptr[(x * 3) + y * stride + 1] = color.G;
    //            ptr[(x * 3) + y * stride + 2] = color.R;
    //        }
    //    }

    //    public Color GetPixel(int x, int y)
    //    {
    //        int stride = _data.Stride;
    //        byte r, g, b, a = 0;
    //        unsafe
    //        {
    //            byte* ptr = (byte*)_data.Scan0;
    //           b = ptr[(x * 3) + y * stride];
    //           g = ptr[(x * 3) + y * stride + 1];
    //           r= ptr[(x * 3) + y * stride + 2];
    //        }
    //        return Color.FromArgb(a,r,g,b);
    //    }

    //    public void RotateFlip(RotateFlipType flipType)
    //    {
    //      //  _bitmap.RotateFlip(flipType);
    //    }

    //    public void Dispose()
    //    {
    //        _bitmap.UnlockBits(_data);
    //    }
    //}
}