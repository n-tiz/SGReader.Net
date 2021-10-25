using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SGReader.Core
{
    public class SGBitmap : IDisposable
    {
        public const int DataSize = 200;

        private readonly string _sgFilePath;
        readonly List<SGImage> _images = new List<SGImage>();
        private FileStream _file;
        private bool _isFileExtern;

        public int Id { get; }
        public SGBitmapData Data { get; }
        public IReadOnlyList<SGImage> Images => _images;
        public string Description { get; }
        public string Name { get; }
        public string FileName => Data.FileName;

        public SGBitmap(int id, string sgFilePath, BinaryReader reader)
        {
            Id = id;
            _sgFilePath = sgFilePath;
            Data = new SGBitmapData(reader);

            Description = $"{Data.FileName} ({Images.Count})";
            Name = Path.GetFileNameWithoutExtension(Data.FileName);
        }

        public void AddImage(SGImage image)
        {
            _images.Add(image);
            image.Parent = this;
        }

        public SGImage GetImageById(int imageId)
        {
            if (imageId < 0 || imageId >= _images.Count)
                return null;
            return _images[imageId];
        }

        public Bitmap CreateImage(int imageId)
        {
            if (imageId < 0 || imageId >= _images.Count)
            {
                return null;
            }

            return _images[imageId].CreateImage();
        }

        public FileStream OpenFile(bool isExtern)
        {
            if (_file != null && _isFileExtern != isExtern)
            {
                CloseFileStream();
            }
            _isFileExtern = isExtern;
            if (_file == null)
            {
                string filename = Get555FileName();
                if (string.IsNullOrEmpty(filename))
                    return null;
                _file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            return _file;
        }

        private string Get555FileName()
        {
            FileInfo fileInfo = new FileInfo(_sgFilePath);

            // Fetch basename of the file
            // either the same name as sg(2|3) or from file record
            var basename = _isFileExtern ? Data.FileName : _sgFilePath;

            // Change the extension to .555
            basename = Path.ChangeExtension(basename, "555");

            string path = FindFilenameCaseInsensitive(fileInfo.Directory, basename);
            if (path != null)
            {
                return path;
            }

            var directory = fileInfo.Directory.GetDirectories().SingleOrDefault(d => d.Name == "555");
            if (directory != null)
            {
                return FindFilenameCaseInsensitive(directory, basename);
            }

            return null;
        }

        private string FindFilenameCaseInsensitive(DirectoryInfo directory, string filename)
        {
            filename = filename.ToLowerInvariant();
            var file = directory.GetFiles().SingleOrDefault(f => f.FullName.ToLowerInvariant() == filename);
            return file?.FullName;
        }

        private void CloseFileStream()
        {
            if (_file == null) return;
            _file.Close();
            _file.Dispose();
            _file = null;
        }

        public void Dispose()
        {
            CloseFileStream();
        }

    }
}