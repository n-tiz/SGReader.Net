using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SGReader.Core
{
    public class SGBitmap : IDisposable
    {
        public const int DataSize = SGFormat.BitmapRecordSize;

        private readonly string _sgFilePath;
        private readonly object _ioLock = new object();
        private readonly List<SGImage> _images = new List<SGImage>();
        private FileStream _file;
        private bool _isFileExtern;

        public int Id { get; }
        public SGBitmapData Data { get; }
        public IReadOnlyList<SGImage> Images => _images;
        public string Description => $"{FileName} ({Images.Count})";
        public string Name { get; }
        public string FileName => Data.FileName;
        public object SyncRoot => _ioLock;

        public SGBitmap(int id, string sgFilePath, BinaryReader reader)
        {
            Id = id;
            _sgFilePath = sgFilePath;
            Data = new SGBitmapData(reader);
            Name = Path.GetFileNameWithoutExtension(Data.FileName);
        }

        public void AddImage(SGImage image)
        {
            _images.Add(image);
            image.Parent = this;
        }

        public FileStream OpenFile(bool isExtern)
        {
            lock (_ioLock)
            {
                if (_file != null && _isFileExtern != isExtern)
                    CloseFileStream();

                _isFileExtern = isExtern;
                if (_file != null)
                    return _file;

                string filename = Resolve555Path();
                if (string.IsNullOrEmpty(filename))
                    return null;

                _file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return _file;
            }
        }

        private string Resolve555Path()
        {
            var sgInfo = new FileInfo(_sgFilePath);
            string basename = _isFileExtern
                ? Path.GetFileName(Data.FileName)
                : Path.GetFileName(_sgFilePath);

            if (string.IsNullOrWhiteSpace(basename))
                return null;

            basename = Path.ChangeExtension(basename, "555");

            string path = FindFileIgnoreCase(sgInfo.Directory, basename);
            if (path != null)
                return path;

            var folder555 = sgInfo.Directory?
                .GetDirectories()
                .FirstOrDefault(d => string.Equals(d.Name, "555", StringComparison.OrdinalIgnoreCase));

            return folder555 == null ? null : FindFileIgnoreCase(folder555, basename);
        }

        private static string FindFileIgnoreCase(DirectoryInfo directory, string filename)
        {
            if (directory == null || !directory.Exists)
                return null;

            string target = Path.GetFileName(filename);
            return directory.GetFiles()
                .FirstOrDefault(f => string.Equals(f.Name, target, StringComparison.OrdinalIgnoreCase))
                ?.FullName;
        }

        private void CloseFileStream()
        {
            _file?.Dispose();
            _file = null;
        }

        public void Dispose()
        {
            lock (_ioLock)
                CloseFileStream();
        }
    }
}
