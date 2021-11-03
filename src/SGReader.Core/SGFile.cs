using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SGReader.Core.Exceptions;

namespace SGReader.Core
{
    public class SGFile : IImageContainer, IDisposable
    {
        private readonly string _filePath;
        private readonly List<SGBitmap> _bitmaps = new List<SGBitmap>();
        private readonly List<SGImage> _images = new List<SGImage>();
        private readonly List<SGAnimation> _animations = new List<SGAnimation>();

        public string Name { get; }

        public IReadOnlyList<SGImage> Images => _images;

        public IReadOnlyList<SGAnimation> Animations => _animations;

        public SGHeader Header { get; private set; }

        public SGIndex Index { get; private set; }

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
                CheckVersion();
                fileStream.Seek(SGHeader.HeaderSize, SeekOrigin.Begin);
                Index = new SGIndex(reader);
                fileStream.Seek(SGHeader.HeaderSize + SGIndex.IndexSize, SeekOrigin.Begin);
                LoadBitmaps(reader);
                fileStream.Seek(SGHeader.HeaderSize + SGIndex.IndexSize + GetMaxBitmapDataCount(Header.Version) * SGBitmap.DataSize, SeekOrigin.Begin);
                LoadImages(reader, Header.Version >= SGFileVersion.SG3FormatWithAlphaMask);
                LoadAnimations();
            }
        }

        private void LoadAnimations()
        {
            _animations.AddRange(SGAnimationFactory.BuildAnimations(this, Index.Entries));
        }


        private void LoadImages(BinaryReader reader, bool includeAlpha)
        {
            // The first image is a dummy/null record
            SGImage dummy = new SGImage(-1, reader, includeAlpha);

            for (int i = 0; i < Header.ImageDataCount; i++)
            {
                var image = CreateImage(reader, includeAlpha, i);
                _images.Add(image);
            }
        }

        private SGImage CreateImage(BinaryReader reader, bool includeAlpha, int i)
        {
            SGImage image = new SGImage(i + 1, reader, includeAlpha);
            int invertOffset = image.InvertOffset;
            if (invertOffset < 0 && (i + invertOffset) >= 0)
            {
                image.SetInvertedImage(_images[i + invertOffset]);
            }

            int bitmapId = image.BitmapId;
            if (bitmapId >= 0 && bitmapId < _bitmaps.Count)
            {
                _bitmaps[bitmapId].AddImage(image);
            }
            return image;
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
            if (Header.Version == SGFileVersion.SG2FormatDemo || Header.Version == SGFileVersion.SG2Format)
            {
                // SG2 file: filesize = 74480 or 522680 (depending on whether it's
                // a "normal" sg2 or an enemy sg2
                if (Header.SGFileSize == 74480 || Header.SGFileSize == 522680)
                {
                    return;
                }
            }
            else if (Header.Version == SGFileVersion.SG3Format || Header.Version == SGFileVersion.SG3FormatWithAlphaMask)
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

        private static int GetMaxBitmapDataCount(SGFileVersion version)
        {
            switch (version)
            {
                case SGFileVersion.SG2FormatDemo:
                    return 50;
                case SGFileVersion.SG2Format:
                    return 100;
                case SGFileVersion.SG3Format:
                case SGFileVersion.SG3FormatWithAlphaMask:
                    return 200;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public SGImage GetImageById(int imageId)
        {
            if (imageId < 0 || imageId >= _images.Count)
            {
                return null;
            }
            return _images.SingleOrDefault(i => i.Id == imageId);
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