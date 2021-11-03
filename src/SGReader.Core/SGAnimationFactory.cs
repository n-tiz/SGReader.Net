using System;
using System.Collections.Generic;
using System.Linq;

namespace SGReader.Core
{
    public static class SGAnimationFactory
    {
        public static List<SGAnimation> BuildAnimations(IImageContainer container,
            IReadOnlyCollection<ushort> indexEntries)
        {
            List<SGAnimation> animations = new List<SGAnimation>();

            foreach (var id in indexEntries.Where(id => id != 0)) //todo : Check les ids !
            {
                var firstImage = container.GetImageById(id);
                animations.AddRange(BuildAnimations(
                    container.Images.Skip(firstImage.Id).Take(firstImage.AnimationSprites * firstImage.Orientations)
                        .ToList(), firstImage.AnimationSprites, firstImage.Orientations));
            }

            return animations;
        }

        private static List<SGAnimation> BuildAnimations(List<SGImage> images, int animationSprites, int orientations)
        {
            List<SGAnimation> animations = new List<SGAnimation>();

            for (int o = 0; o < orientations; o++)
            {
                List<SGImage> animationsImages = new List<SGImage>();
                for (int a = 0; a < animationSprites; a++)
                {
                    var image = images.ElementAt(o + a * orientations);
                    Console.WriteLine($"{o + a * orientations} : {image.Id - images.First().Id}");
                    animationsImages.Add(image);
                }

                animations.Add(new SGAnimation(animationsImages));
            }

            return animations;
        }

    }
}