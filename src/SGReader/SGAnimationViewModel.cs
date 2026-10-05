using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SGReader.Core;
using SGReader.Helpers;

namespace SGReader
{
    public partial class SGAnimationViewModel : ObservableObject
    {
        private readonly SGAnimation _animation;
        private IReadOnlyList<SGImageViewModel> _sprites;

        public IReadOnlyList<SGImageViewModel> Sprites => _sprites ??= BuildSprites();

        public SGImageViewModel Preview => Sprites.Count > 0 ? Sprites[0] : null;

        public int Count => _animation.Images.Count;

        public string Title { get; }
        public string Description => $"{Count} frames";
        public string FullDescription => string.Join(" → ", _animation.Images.Select(s => s.Id));
        public string BitmapName => _animation.Images.FirstOrDefault()?.Parent?.FileName ?? "—";
        public string SizeLabel => Preview == null ? "—" : Preview.Description;
        public string TypeLabel { get; }

        public bool IsReversible { get; }

        [ObservableProperty]
        private bool _isSelected;

        public SGAnimationViewModel(SGAnimation animation)
        {
            _animation = animation;
            var first = _animation.Images.FirstOrDefault();
            Title = string.IsNullOrWhiteSpace(_animation.Name)
                ? (first?.Parent?.FileName ?? $"Animation {first?.Id}")
                : _animation.Name;
            TypeLabel = SGImageTypeHelper.FormatTypes(_animation.Images.Select(i => i.Type));
            IsReversible = first?.IsAnimationReversible == true;
        }

        private IReadOnlyList<SGImageViewModel> BuildSprites()
        {
            var images = _animation.Images;
            if (images.Count == 0)
                return Array.Empty<SGImageViewModel>();

            // Julius figure draw: pixel -= sprite_offset, so the hotspot inside the
            // bitmap is at (XOffset, YOffset). Keep that point fixed across frames.
            bool hasOffsets = images.Any(i => i.XOffset != 0 || i.YOffset != 0);
            if (!hasOffsets)
            {
                int maxW = images.Max(i => i.Width);
                int maxH = images.Max(i => i.Height);
                return images
                    .Select(image => new SGImageViewModel(
                        image,
                        maxW,
                        maxH,
                        (maxW - image.Width) / 2,
                        maxH - image.Height))
                    .ToList();
            }

            int hotspotX = images.Max(i => i.XOffset);
            int hotspotY = images.Max(i => i.YOffset);
            int canvasWidth = images.Max(i => hotspotX - i.XOffset + i.Width);
            int canvasHeight = images.Max(i => hotspotY - i.YOffset + i.Height);
            canvasWidth = Math.Max(1, canvasWidth);
            canvasHeight = Math.Max(1, canvasHeight);

            return images
                .Select(image => new SGImageViewModel(
                    image,
                    canvasWidth,
                    canvasHeight,
                    hotspotX - image.XOffset,
                    hotspotY - image.YOffset))
                .ToList();
        }
    }
}
