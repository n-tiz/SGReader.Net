using System.Globalization;
using System.IO;

namespace SGReader.Core
{
    public class SGFile
    {
        private readonly string _filePath;

        public string Name => TextHelper.CleanFileName(_filePath);
        
        public int ImagesCount => 42;

        public SGFile(string filePath)
        {
            _filePath = filePath;
        }
    }
}