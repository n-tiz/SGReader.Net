using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        public string FullDescription
        {
            get
            {
                var ids = _animation.Images.Select(s => s.Id.ToString());
                if (_animation.BaseImage != null)
                    return $"{_animation.BaseImage.Id} + {string.Join(" → ", ids)}";
                return string.Join(" → ", ids);
            }
        }

        public string BitmapName =>
            _animation.BaseImage?.Parent?.FileName
            ?? _animation.Images.FirstOrDefault()?.Parent?.FileName
            ?? "—";

        public string SizeLabel => Preview == null ? "—" : Preview.Description;

        public string TypeLabel { get; }

        public bool IsReversible { get; }

        [ObservableProperty]
        private bool _isSelected;

        public SGAnimationViewModel(SGAnimation animation)
        {
            _animation = animation;
            var first = _animation.Images.FirstOrDefault();
            var baseImage = _animation.BaseImage;
            Title = string.IsNullOrWhiteSpace(_animation.Name)
                ? (baseImage?.Parent?.FileName ?? first?.Parent?.FileName ?? $"Animation {first?.Id}")
                : _animation.Name;

            var types = _animation.Images.Select(i => i.Type).AsEnumerable();
            if (baseImage != null)
                types = new[] { baseImage.Type }.Concat(types);
            TypeLabel = SGImageTypeHelper.FormatTypes(types);

            IsReversible = baseImage?.IsAnimationReversible == true
                || first?.IsAnimationReversible == true;
        }

        private IReadOnlyList<SGImageViewModel> BuildSprites()
        {
            var images = _animation.Images;
            if (images.Count == 0)
                return Array.Empty<SGImageViewModel>();

            if (_animation.BaseImage != null)
                return BuildCompositedSprites(_animation.BaseImage, images);

            return BuildAlignedSprites(images);
        }

        private static IReadOnlyList<SGImageViewModel> BuildCompositedSprites(
            SGImage baseImage,
            IReadOnlyList<SGImage> frames)
        {
            using var baseBitmap = baseImage.CreateImage();
            if (baseBitmap == null)
                return BuildAlignedSprites(frames);

            int offsetX = baseImage.XOffset;
            int offsetY = baseImage.YOffset;
            var result = new List<SGImageViewModel>(frames.Count);

            foreach (var frame in frames)
            {
                try
                {
                    using var overlay = frame.CreateImage();
                    if (overlay == null)
                        continue;

                    int canvasWidth = Math.Max(baseBitmap.Width, offsetX + overlay.Width);
                    int canvasHeight = Math.Max(baseBitmap.Height, offsetY + overlay.Height);
                    using var canvas = new Bitmap(canvasWidth, canvasHeight, PixelFormat.Format32bppArgb);
                    using (var graphics = Graphics.FromImage(canvas))
                    {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImageUnscaled(baseBitmap, 0, 0);
                        graphics.DrawImageUnscaled(overlay, offsetX, offsetY);
                    }

                    result.Add(SGImageViewModel.FromRendered(frame, canvas));
                }
                catch
                {
                    // Skip frames that fail to decode.
                }
            }

            return result;
        }

        private static IReadOnlyList<SGImageViewModel> BuildAlignedSprites(IReadOnlyList<SGImage> images)
        {
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
            int canvasWidth = Math.Max(1, images.Max(i => hotspotX - i.XOffset + i.Width));
            int canvasHeight = Math.Max(1, images.Max(i => hotspotY - i.YOffset + i.Height));

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
