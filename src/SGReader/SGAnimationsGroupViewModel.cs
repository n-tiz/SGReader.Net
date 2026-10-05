using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SGReader.Core;

namespace SGReader
{
    public class SGAnimationsGroupViewModel : ObservableObject
    {
        public IReadOnlyCollection<SGAnimationViewModel> Animations { get; }

        public string Name { get; }

        public string Description { get; }

        public SGAnimationsGroupViewModel(SGAnimationsGroup animationsGroup)
            : this(
                animationsGroup.Animations.Select(animation => new SGAnimationViewModel(animation)).ToList(),
                animationsGroup.Animations.FirstOrDefault() is { } first
                    ? new SGAnimationViewModel(first).Title
                    : "Animation group",
                $"{animationsGroup.Orientations} orientations · {animationsGroup.SpritesByAnimation} frames")
        {
        }

        private SGAnimationsGroupViewModel(IReadOnlyCollection<SGAnimationViewModel> animations, string name, string description)
        {
            Animations = animations;
            Name = name;
            Description = description;
        }

        public static SGAnimationsGroupViewModel FromBitmap(SGBitmap bitmap, ISet<int> excludeImageIds = null)
        {
            var animations = bitmap.Images
                .Where(image => image.Width > 0 && image.Height > 0)
                .Where(image => excludeImageIds == null || !excludeImageIds.Contains(image.Id))
                .Select(image =>
                {
                    var animation = new SGAnimation(new List<SGImage> { image })
                    {
                        Name = $"{bitmap.Name} #{image.Id}"
                    };
                    return new SGAnimationViewModel(animation);
                })
                .ToList();

            if (animations.Count == 0)
                return null;

            return new SGAnimationsGroupViewModel(
                animations,
                string.IsNullOrWhiteSpace(bitmap.Name) ? $"Bitmap {bitmap.Id}" : bitmap.Name,
                $"{animations.Count} images");
        }
    }
}
