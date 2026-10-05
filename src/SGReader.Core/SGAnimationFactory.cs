using System.Collections.Generic;
using System.Linq;

namespace SGReader.Core
{
    public static class SGAnimationFactory
    {
        public static List<SGAnimationsGroup> BuildAnimationsGroup(
            IImageContainer container,
            IReadOnlyCollection<ushort> indexEntries)
        {
            var animations = new List<SGAnimationsGroup>();

            foreach (var id in indexEntries.Where(id => id != 0))
            {
                var firstImage = container.GetImageById(id);
                if (firstImage == null || firstImage.AnimationSprites <= 0)
                    continue;

                var group = firstImage.Orientations > 0
                    ? BuildOrientedAnimations(container, firstImage)
                    : BuildConsecutiveAnimation(container, firstImage);

                if (group.Animations.Count > 0)
                    animations.Add(group);
            }

            return animations;
        }

        /// <summary>
        /// Walkers / multi-facing: frame layout is base + direction + orientations * frame.
        /// </summary>
        private static SGAnimationsGroup BuildOrientedAnimations(IImageContainer container, SGImage firstImage)
        {
            var animations = new List<SGAnimation>();
            int orientations = firstImage.Orientations;
            int sprites = firstImage.AnimationSprites;

            for (int o = 0; o < orientations; o++)
            {
                var frames = new List<SGImage>();
                for (int a = 0; a < sprites; a++)
                {
                    var image = container.GetImageById(firstImage.Id + o + a * orientations);
                    if (image != null && image.Width > 0 && image.Height > 0)
                        frames.Add(image);
                }

                if (frames.Count > 0)
                    animations.Add(new SGAnimation(frames));
            }

            return new SGAnimationsGroup(orientations, sprites, animations);
        }

        /// <summary>
        /// Buildings / effects: head image (often isometric) declares N sprites; the next N
        /// records are the animation frames (Orientations == 0 in the SG metadata).
        /// </summary>
        private static SGAnimationsGroup BuildConsecutiveAnimation(IImageContainer container, SGImage firstImage)
        {
            var frames = new List<SGImage>();
            for (int a = 1; a <= firstImage.AnimationSprites; a++)
            {
                var image = container.GetImageById(firstImage.Id + a);
                if (image != null && image.Width > 0 && image.Height > 0)
                    frames.Add(image);
            }

            if (frames.Count == 0)
                return new SGAnimationsGroup(0, 0, new List<SGAnimation>());

            string name = firstImage.Parent?.FileName;
            if (string.IsNullOrWhiteSpace(name))
                name = $"Animation {firstImage.Id}";

            // Isometric heads use AnimationSprites as overlays drawn on top of the base.
            // Other consecutive strips just play the following frames as-is.
            bool compositeOnBase = firstImage.Type == 30;

            return new SGAnimationsGroup(
                orientations: 1,
                spritesByAnimation: frames.Count,
                animations: new List<SGAnimation>
                {
                    new SGAnimation(frames)
                    {
                        Name = name,
                        BaseImage = compositeOnBase ? firstImage : null
                    }
                });
        }
    }
}
