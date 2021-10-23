﻿using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Collections.Generic;

namespace SGReader.Core
{
    public class SGFile : IDisposable
    {
        private readonly string _filePath;
        private readonly List<SGBitmap> _bitmaps = new List<SGBitmap>();
        private readonly List<SGImage> _images = new List<SGImage>();

        public string Name { get; }
        public IReadOnlyList<SGBitmap> Bitmaps => _bitmaps;
        public IReadOnlyList<SGImage> Images => _images;

        public SGHeader Header { get; private set; }

        public SGFile(string filePath)
        {
            _filePath = filePath;
            Name = TextHelper.CleanFileName(_filePath);
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

        public int GetImagesCountForBitmap(int bitmapId)
        {
            if (bitmapId < 0 || bitmapId >= _bitmaps.Count)
                return -1;
            return _bitmaps[bitmapId].Images.Count;
        }

        public SGImage GetImageById(int imageId)
        {
            if (imageId < 0 || imageId >= _images.Count)
            {
                return null;
            }
            return _images[imageId];
        }

        public SGImage GetImageForBitmap(int bitmapId, int imageId)
        {
            if (bitmapId < 0 || bitmapId >= _bitmaps.Count || imageId < 0 || imageId >= _bitmaps[bitmapId].Images.Count)
            {
                return null;
            }

            return _bitmaps[bitmapId].Images[imageId];
        }

        public Image CreateImage(int imageId)
        {
            if (imageId < 0 || imageId >= Images.Count)
            {
                return null;
            }
            return _images[imageId].CreateImage();
        }
        
        public Image CreateImageForBitmap(int bitmapId, int imageId)
        {
            if (bitmapId < 0 || bitmapId >= _bitmaps.Count ||
                imageId < 0 || imageId >= _bitmaps[bitmapId].Images.Count)
            {
                return null;
            }
            return _bitmaps[bitmapId].CreateImage(imageId);
        }

        public SGBitmap GetBitmapById(int bitmapId)
        {
            if (bitmapId < 0 || bitmapId >= _bitmaps.Count)
            {
                return null;
            }

            return _bitmaps[bitmapId];
        }

        public string GetBitmapDescription(int bitmapId)
        {
            return GetBitmapById(bitmapId)?.Description;
        }

        public void Dispose()
        {
            foreach (var bitmap in _bitmaps)
            {
                bitmap.Dispose();
            }
            _bitmaps.Clear();
            foreach (var image in _images)
            {
                image.Dispose();
            }
        }
    }
}