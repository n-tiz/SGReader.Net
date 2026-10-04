using System.Collections.Generic;
using System.Linq;

namespace SGReader.Core
{
    public static class SGAnimationFactory
    {
        public static List<SGAnimationsGroup> BuildAnimationsGroup(IImageContainer container, IReadOnlyCollection<ushort> indexEntries)
        {
            var animations = new List<SGAnimationsGroup>();

            foreach (var id in indexEntries.Where(id => id != 0))
            {
                var firstImage = container.GetImageById(id);
                if (firstImage == null)
                    continue;

                // Static/stat graphics often have index entries but no orientation/frame metadata.
                if (firstImage.Orientations <= 0 || firstImage.AnimationSprites <= 0)
                    continue;

                var group = BuildAnimations(container, firstImage);
                if (group.Animations.Count > 0)
                    animations.Add(group);
            }

            return animations;
        }

        private static SGAnimationsGroup BuildAnimations(IImageContainer container, SGImage firstImage)
        {
            var animations = new List<SGAnimation>();

            for (int o = 0; o < firstImage.Orientations; o++)
            {
                var animationImages = new List<SGImage>();
                for (int a = 0; a < firstImage.AnimationSprites; a++)
                {
                    var image = container.GetImageById(firstImage.Id + o + a * firstImage.Orientations);
                    if (image != null && image.Width > 0 && image.Height > 0)
                        animationImages.Add(image);
                }

                if (animationImages.Count > 0)
                    animations.Add(new SGAnimation(animationImages));
            }

            return new SGAnimationsGroup(firstImage.Orientations, firstImage.AnimationSprites, animations);
        }
    }
}
