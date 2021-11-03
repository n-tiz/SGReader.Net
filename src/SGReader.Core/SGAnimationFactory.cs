using System;
using System.Collections.Generic;
using System.Linq;

namespace SGReader.Core
{
    public static class SGAnimationFactory
    {
        public static List<SGAnimation> BuildAnimations(IImageContainer container, IReadOnlyCollection<ushort> indexEntries)
        {
            List<SGAnimation> animations = new List<SGAnimation>();

            foreach (var id in indexEntries.Where(id => id != 0)) //todo : Check les ids !
            {
                var firstImage = container.GetImageById(id);
                animations.AddRange(BuildAnimations(container, firstImage));
            }

            return animations;
        }

        private static IEnumerable<SGAnimation> BuildAnimations(IImageContainer container, SGImage firstImage)
        {
            List<SGAnimation> animations = new List<SGAnimation>();

            for (int o = 0; o < firstImage.Orientations; o++)
            {
                List<SGImage> animationsImages = new List<SGImage>();
                for (int a = 0; a < firstImage.AnimationSprites; a++)
                {
                    var image = container.GetImageById(firstImage.Id + o + a * firstImage.Orientations);
//                    Console.WriteLine($"{o + a * orientations} : {image.Id - images.First().Id}");
                    animationsImages.Add(image);
                }

                animations.Add(new SGAnimation(animationsImages));
            }

            return animations;
        }

    }
}