using System;
using System.Collections.Generic;
using System.Linq;

namespace SGReader.Core
{
    public static class SGAnimationFactory
    {
        // Field at SG offset 32 is undocumented; Julius skips it and hardcodes
        // 8 directions for walkers: image_id = base + direction + 8 * frame.
        private const int MaxOrientations = 16;
        private const int WalkerDirections = 8;

        public static List<SGAnimationsGroup> BuildAnimationsGroup(IImageContainer container, IReadOnlyCollection<ushort> indexEntries)
        {
            var animations = new List<SGAnimationsGroup>();

            foreach (var id in indexEntries.Where(id => id != 0))
            {
                var firstImage = container.GetImageById(id);
                if (firstImage == null)
                    continue;

                if (firstImage.AnimationSprites <= 0)
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
            int sprites = firstImage.AnimationSprites;
            int orientations = ResolveOrientations(container, firstImage);

            for (int o = 0; o < orientations; o++)
            {
                var animationImages = new List<SGImage>();
                for (int a = 0; a < sprites; a++)
                {
                    // Walker layout (Julius): base + direction + orientations * frame
                    // Building layout: orientations == 1 → consecutive frames
                    var image = container.GetImageById(firstImage.Id + o + a * orientations);
                    if (image != null && image.Width > 0 && image.Height > 0)
                        animationImages.Add(image);
                }

                if (animationImages.Count > 0)
                {
                    animations.Add(new SGAnimation(animationImages)
                    {
                        Name = orientations > 1
                            ? $"{firstImage.Parent?.FileName ?? "Anim"} · dir {o + 1}"
                            : firstImage.Parent?.FileName
                    });
                }
            }

            return new SGAnimationsGroup(orientations, sprites, animations);
        }

        private static int ResolveOrientations(IImageContainer container, SGImage firstImage)
        {
            // Prefer walker detection over the undocumented SG field — that field is often
            // wrong/absent and would produce only a subset of facings that look correct.
            if (LooksLikeWalkerSet(container, firstImage))
                return WalkerDirections;

            int raw = firstImage.Orientations;
            if (raw >= 2 && raw <= MaxOrientations)
                return raw;

            return 1;
        }

        private static bool LooksLikeWalkerSet(IImageContainer container, SGImage firstImage)
        {
            int sprites = firstImage.AnimationSprites;
            if (sprites < 2)
                return false;

            int lastId = firstImage.Id + WalkerDirections * sprites - 1;
            if (container.GetImageById(lastId) == null)
                return false;

            SGBitmap parent = firstImage.Parent;
            int typedSprites = 0;
            int mirrored = 0;

            for (int d = 0; d < WalkerDirections; d++)
            {
                var img = container.GetImageById(firstImage.Id + d);
                if (img == null || img.Width <= 0 || img.Height <= 0)
                    return false;
                if (parent != null && img.Parent != null && img.Parent != parent)
                    return false;

                // Walkers are transparent sprites; allow mirrored facings (InvertOffset).
                if (img.Type == 0)
                    typedSprites++;
                if (img.IsInverted)
                    mirrored++;

                // Hotspot should exist on at least the non-mirrored source facings.
                if (!img.IsInverted && img.XOffset == 0 && img.YOffset == 0)
                    return false;
            }

            if (typedSprites < WalkerDirections)
                return false;

            var nextFrame = container.GetImageById(firstImage.Id + WalkerDirections);
            if (nextFrame == null || nextFrame.Type != 0 || nextFrame.Width <= 0)
                return false;
            if (parent != null && nextFrame.Parent != null && nextFrame.Parent != parent)
                return false;

            // Mirrored facings are a strong walker signal (dirs 4–7 often flip 0–3).
            // Even without mirrors, an 8-wide type-0 strip with hotspots is enough.
            return mirrored > 0 || sprites >= 2;
        }
    }
}
