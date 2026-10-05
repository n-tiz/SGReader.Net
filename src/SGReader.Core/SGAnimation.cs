using System.Collections.Generic;

namespace SGReader.Core
{
    public class SGAnimation
    {
        private readonly List<SGImage> _images;

        public IReadOnlyList<SGImage> Images => _images;

        /// <summary>
        /// Optional underlay (typically the isometric head image). Animation sprites are
        /// drawn on top at the head's animation sprite offsets.
        /// </summary>
        public SGImage BaseImage { get; set; }

        public string Name { get; set; }

        public SGAnimation(List<SGImage> images)
        {
            _images = images;
        }
    }
}
