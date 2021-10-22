using System.IO;

namespace SGReader.Core
{
    public class SGImage
    {
        private SGImageData _workData;
        private SGImageData _data;

        public int Id { get; }
        public bool IsInverted { get; set; }
        public SGImage InvertedImage { get; set; }
        public SGBitmap Parent { get; set; } = null;
        public int InvertOffset => _data.InvertOffset;

        public int BitmapId => _workData?.BitmapId ?? _data.BitmapId;

        public SGImage(int id, BinaryReader reader, bool includeAlpha)
        {
            Id = id;
            _workData = new SGImageData(reader, includeAlpha);
            _data = _workData;
            IsInverted = _data.InvertOffset != 0;
        }
    }
}