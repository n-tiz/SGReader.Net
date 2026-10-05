using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SGReader.Core.Exceptions;

namespace SGReader.Core
{
    public class SGFile : IImageContainer, IDisposable
    {
        private readonly string _filePath;
        private readonly List<SGBitmap> _bitmaps = new List<SGBitmap>();
        private readonly List<SGImage> _images = new List<SGImage>();
        private readonly List<SGAnimationsGroup> _animationsGroups = new List<SGAnimationsGroup>();

        public string Name { get; }
        public IReadOnlyList<SGImage> Images => _images;
        public IReadOnlyList<SGBitmap> Bitmaps => _bitmaps;
        public IReadOnlyList<SGAnimationsGroup> AnimationsGroups => _animationsGroups;
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

            using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new BinaryReader(fileStream, Encoding.Latin1);

            Header = new SGHeader(reader);
            EnsureValidVersion();

            fileStream.Seek(SGFormat.HeaderSize, SeekOrigin.Begin);
            Index = new SGIndex(reader);

            fileStream.Seek(SGFormat.HeaderSize + SGFormat.IndexSize, SeekOrigin.Begin);
            LoadBitmaps(reader);

            int bitmapSectionSize = SGFormat.MaxBitmapSlots(Header.Version) * SGFormat.BitmapRecordSize;
            fileStream.Seek(SGFormat.HeaderSize + SGFormat.IndexSize + bitmapSectionSize, SeekOrigin.Begin);
            LoadImages(reader, Header.Version >= SGFileVersion.SG3FormatWithAlphaMask);

            _animationsGroups.AddRange(SGAnimationFactory.BuildAnimationsGroup(this, Index.Entries));
        }

        private void LoadImages(BinaryReader reader, bool includeAlpha)
        {
            // Record 0 is a dummy / null image and is discarded.
            _ = new SGImage(-1, reader, includeAlpha);

            for (int i = 0; i < Header.ImageDataCount; i++)
            {
                var image = new SGImage(i + 1, reader, includeAlpha);

                int invertOffset = image.InvertOffset;
                if (invertOffset < 0 && i + invertOffset >= 0)
                    image.SetInvertedImage(_images[i + invertOffset]);

                int bitmapId = image.BitmapId;
                if (bitmapId >= 0 && bitmapId < _bitmaps.Count)
                    _bitmaps[bitmapId].AddImage(image);

                _images.Add(image);
            }
        }

        private void LoadBitmaps(BinaryReader reader)
        {
            for (int i = 0; i < Header.BitmapDataCount; i++)
                _bitmaps.Add(new SGBitmap(i, _filePath, reader));
        }

        private void EnsureValidVersion()
        {
            switch (Header.Version)
            {
                case SGFileVersion.SG2FormatDemo:
                case SGFileVersion.SG2Format:
                    if (Header.SGFileSize == SGFormat.Sg2NormalFileSize
                        || Header.SGFileSize == SGFormat.Sg2EnemyFileSize)
                        return;
                    break;

                case SGFileVersion.SG3Format:
                case SGFileVersion.SG3FormatWithAlphaMask:
                    // Some SG3 files reuse the SG2 sentinel; otherwise size must match the file.
                    if (Header.SGFileSize == SGFormat.Sg2NormalFileSize
                        || new FileInfo(_filePath).Length == Header.SGFileSize)
                        return;
                    break;
            }

            throw new InvalidSGFileException(
                $"File version ({Header.Version}) or file size is not valid.", this);
        }

        public SGImage GetImageById(int imageId)
        {
            // Image IDs are 1-based; dummy record at position 0 was discarded on load.
            if (imageId < 1 || imageId > _images.Count)
                return null;

            return _images[imageId - 1];
        }

        public void Dispose()
        {
            foreach (var bitmap in _bitmaps)
                bitmap.Dispose();
            _bitmaps.Clear();

            foreach (var image in _images)
                image.Dispose();
            _images.Clear();
        }
    }
}
