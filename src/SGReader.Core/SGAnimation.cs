using System.Collections.Generic;

namespace SGReader.Core
{
    public class SGAnimation
    {
        private readonly List<SGImage> _images;

        public SGBitmap Parent { get; }
        
        public IReadOnlyList<SGImage> Images => _images;
        
        public string Name { get; set; }

        public SGAnimation(SGBitmap parent, List<SGImage> images)
        {
            Parent = parent;
            _images = images;
        }
    }
}