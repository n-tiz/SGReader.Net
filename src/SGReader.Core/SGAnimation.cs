using System.Collections.Generic;

namespace SGReader.Core
{
    public class SGAnimation
    {
        private readonly List<SGImage> _images;
       
        public IReadOnlyList<SGImage> Images => _images;
        
        public string Name { get; set; }

        public SGAnimation(List<SGImage> images)
        {
            _images = images;
        }
    }
}