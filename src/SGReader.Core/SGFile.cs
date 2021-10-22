using System.Globalization;
using System.IO;
using System.Text;

namespace SGReader.Core
{
    public class SGFile
    {
        private readonly string _filePath;

        public string Name => TextHelper.CleanFileName(_filePath);

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
            }
        }

    }
}