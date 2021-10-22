using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SGReader.Core
{
    public class SGFile
    {
        private readonly string _filePath;
        private readonly List<SGBitmap> _bitmaps = new List<SGBitmap>();
        private readonly List<SGImage> _images = new List<SGImage>();

        public string Name => TextHelper.CleanFileName(_filePath);
        public IReadOnlyCollection<SGBitmap> Bitmaps => _bitmaps;
        public IReadOnlyCollection<SGImage> Images => _images;

        public int ImagesCount => 42;
        public SGHeader Header { get; private set; }

        public SGFile(string filePath)
        {
            _filePath = filePath;
        }

        public void Load()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException(_filePath);
            using (FileStream fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BinaryReader reader = new BinaryReader(fileStream, Encoding.Default))
            {
                Header = new SGHeader(reader);
                fileStream.Seek(SGHeader.HeaderSize, SeekOrigin.Begin);
                CheckVersion();
                LoadBitmaps(reader);
                fileStream.Seek(SGHeader.HeaderSize + GetMaxBitmapDataCount() * SGBitmap.DataSize, SeekOrigin.Begin);
                LoadImages(reader, Header.Version >= 0xD6);
            }
        }

        private void LoadImages(BinaryReader reader, bool includeAlpha)
        {
            // The first image is a dummy/null record
            SGImage dummy = new SGImage(-1, reader, includeAlpha);

            for (int i = 0; i < Header.ImageDataCount; i++)
            {
                SGImage image = new SGImage(i + 1, reader, includeAlpha);
                int invertOffset = image.InvertOffset;
                if (invertOffset < 0 && (i + invertOffset) >= 0)
                {
                    image.InvertedImage = _images[i + invertOffset];
                }

                int bitmapId = image.BitmapId;
                if (bitmapId >= 0 && bitmapId < _bitmaps.Count)
                {
                    _bitmaps[bitmapId].AddImage(image);
                    image.Parent = _bitmaps[bitmapId];
                }
                _images.Add(image);
            }
        }

        private void LoadBitmaps(BinaryReader reader)
        {
            for (int i = 0; i < Header.BitmapDataCount; i++)
            {
                SGBitmap bitmap = new SGBitmap(i, _filePath, reader);
                _bitmaps.Add(bitmap);
            }
        }

        private void CheckVersion()
        {
            if (Header.Version == 0xcf || Header.Version == 0xd3)
            {
                // SG2 file: filesize = 74480 or 522680 (depending on whether it's
                // a "normal" sg2 or an enemy sg2
                if (Header.SGFileSize == 74480 || Header.SGFileSize == 522680)
                {
                    return;
                }
            }
            else if (Header.Version == 0xd5 || Header.Version == 0xd6)
            {
                // SG3 file: filesize = the actual size of the sg3 file
                FileInfo fi = new FileInfo(_filePath);
                if (Header.SGFileSize == 74480 || fi.Length == Header.SGFileSize)
                {
                    return;
                }
            }
            // All other cases:
            throw new InvalidSGFileException($"File version ({Header.Version}) or file size is not valid.", this);
        }

        private int GetMaxBitmapDataCount()
        {
            switch (Header.Version)
            {
                case 0xcf:
                    return 50; // C3 demo SG2
                case 0xd3:
                    return 100; // SG2
                default:
                    return 200; // SG3
            }
        }
    }

    public class SGBitmap : IDisposable
    {
        private readonly string _filePath;
        List<SGImage> _images = new List<SGImage>();

        public int Id { get; }
        public SGBitmapData Data { get; }
        public const int DataSize = 200;

        public SGBitmap(int id, string filePath, BinaryReader reader)
        {
            _filePath = filePath;
            Id = id;
            Data = new SGBitmapData(reader);
        }

        public void Dispose()
        {
            
        }

        public void AddImage(SGImage image)
        {
            _images.Add(image);
        }
    }
}