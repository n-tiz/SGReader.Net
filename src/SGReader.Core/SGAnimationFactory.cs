using System;
using System.Collections.Generic;
using System.Linq;

namespace SGReader.Core
{
    public static class SGAnimationFactory
    {
        public static List<SGAnimationsGroup> BuildAnimationsGroup(IImageContainer container, IReadOnlyCollection<ushort> indexEntries)
        {
            List<SGAnimationsGroup> animations = new List<SGAnimationsGroup>();

            foreach (var id in indexEntries.Where(id => id != 0))
            {
                var firstImage = container.GetImageById(id);
                animations.Add(BuildAnimations(container, firstImage));
            }

            return animations;
        }

        private static SGAnimationsGroup BuildAnimations(IImageContainer container, SGImage firstImage)
        {
            List<SGAnimation> animations = new List<SGAnimation>();

            for (int o = 0; o < firstImage.Orientations; o++)
            {
                List<SGImage> animationsImages = new List<SGImage>();
                for (int a = 0; a < firstImage.AnimationSprites; a++)
                {
                    var image = container.GetImageById(firstImage.Id + o + a * firstImage.Orientations);
                    animationsImages.Add(image);
                }

                animations.Add(new SGAnimation(animationsImages));
            }

            return new SGAnimationsGroup(firstImage.Orientations, firstImage.AnimationSprites, animations);
        }

    }
}