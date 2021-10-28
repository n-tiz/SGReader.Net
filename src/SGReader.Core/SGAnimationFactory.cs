using System.Collections.Generic;
using System.Linq;

namespace SGReader.Core
{
    public static class SGAnimationFactory
    {
        public static List<SGAnimation> BuildAnimations(SGBitmap bitmap)
        {
            List<SGAnimation> animations = new List<SGAnimation>();

            for (var i = 0; i < bitmap.Images.Count; i++)
            {
                var image = bitmap.Images[i];
                if (image.AnimationSprites != 0 && image.Orientations != 0)
                {
                    animations.AddRange(BuildAnimations(bitmap, bitmap.Images.Skip(i).Take(image.AnimationSprites * image.Orientations).ToList(), image.AnimationSprites, image.Orientations));
                    i += image.AnimationSprites * image.Orientations;
                }
            }

            return animations;
        }

        private static List<SGAnimation> BuildAnimations(SGBitmap bitmap, List<SGImage> images, int animationSprites, int orientations)
        {
            List<SGAnimation> animations = new List<SGAnimation>();

            for (int o = 0; o < orientations; o++)
            {
                List<SGImage> animationsImages = new List<SGImage>();
                for (int a = 0; a < animationSprites; a++)
                {
                    animationsImages.Add(images.ElementAt(o + a * orientations));
                }
                animations.Add(new SGAnimation(bitmap, animationsImages));
            }
            return animations;
        }
    }
}