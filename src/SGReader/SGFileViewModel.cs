using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SGReader.Animations;
using SGReader.Core;
using SGReader.Helpers;

namespace SGReader
{
    public partial class SGFileViewModel : ObservableObject, IDisposable
    {
        private readonly SGFile _sgFile;
        private AnimationPlayerViewModel _animationPlayer;

        public SGFileViewModel(SGFile sgFile)
        {
            _sgFile = sgFile;
            Name = _sgFile.Name;
            VersionLabel = FormatVersion(_sgFile.Header.Version);
            ImageCount = _sgFile.Images.Count;
            TypesLabel = SGImageTypeHelper.FormatTypes(_sgFile.Images.Select(i => i.Type));

            foreach (var animation in sgFile.AnimationsGroups)
            {
                var group = new SGAnimationsGroupViewModel(animation);
                if (group.Animations.Count > 0)
                    AnimationsGroups.Add(group);
            }

            // Stat / static SG files have images but no usable animation metadata.
            if (AnimationsGroups.Count == 0)
            {
                foreach (var bitmap in sgFile.Bitmaps.Where(b => b.Images.Any(i => i.Width > 0 && i.Height > 0)))
                {
                    var group = SGAnimationsGroupViewModel.FromBitmap(bitmap);
                    if (group.Animations.Count > 0)
                        AnimationsGroups.Add(group);
                }
            }

            AnimationGroupCount = AnimationsGroups.Count;
            Description = AnimationsGroups.Count > 0 && sgFile.AnimationsGroups.Count == 0
                ? $"{AnimationGroupCount} bitmaps · {ImageCount} images"
                : $"{AnimationGroupCount} groups · {ImageCount} images";

            Thumbnail = PickThumbnail();
            SelectedAnimation = AnimationsGroups.SelectMany(g => g.Animations).FirstOrDefault();
        }

        public AnimationPlayerViewModel AnimationPlayer => _animationPlayer ??= CreateAnimationPlayer();

        public ObservableCollection<SGAnimationsGroupViewModel> AnimationsGroups { get; } = new ObservableCollection<SGAnimationsGroupViewModel>();

        public BitmapImage Thumbnail { get; }

        public bool HasThumbnail => Thumbnail != null;

        [ObservableProperty]
        private SGAnimationViewModel _selectedAnimation;

        partial void OnSelectedAnimationChanged(SGAnimationViewModel value)
        {
            if (_animationPlayer != null)
                _animationPlayer.Animation = value;
        }

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _description;

        public string VersionLabel { get; }

        public string TypesLabel { get; }

        public int ImageCount { get; }

        public int AnimationGroupCount { get; }

        public void Dispose()
        {
            _animationPlayer?.Dispose();
            _sgFile?.Dispose();
        }

        private AnimationPlayerViewModel CreateAnimationPlayer()
        {
            var player = new AnimationPlayerViewModel
            {
                Animation = SelectedAnimation
            };
            return player;
        }

        private BitmapImage PickThumbnail()
        {
            // Only decode animation previews until we find a usable one.
            foreach (var animation in AnimationsGroups.SelectMany(group => group.Animations))
            {
                var preview = animation.Preview;
                if (preview?.Bitmap != null)
                    return preview.Bitmap;
            }

            foreach (var image in _sgFile.Images)
            {
                if (image.Width <= 0 || image.Height <= 0)
                    continue;

                try
                {
                    using var bitmap = image.CreateImage();
                    if (bitmap == null || IsMostlyEmpty(bitmap))
                        continue;

                    return SGImageViewModel.ToBitmapImage(bitmap);
                }
                catch
                {
                    // Keep looking for a usable frame.
                }
            }

            return null;
        }

        private static bool IsMostlyEmpty(System.Drawing.Bitmap bitmap)
        {
            int opaque = 0;
            int samples = 0;
            int stepX = Math.Max(1, bitmap.Width / 8);
            int stepY = Math.Max(1, bitmap.Height / 8);

            for (int y = 0; y < bitmap.Height; y += stepY)
            {
                for (int x = 0; x < bitmap.Width; x += stepX)
                {
                    samples++;
                    if (bitmap.GetPixel(x, y).A > 16)
                        opaque++;
                }
            }

            return samples == 0 || opaque == 0;
        }

        private static string FormatVersion(SGFileVersion version) => version switch
        {
            SGFileVersion.SG2FormatDemo => "SG2 Demo",
            SGFileVersion.SG2Format => "SG2",
            SGFileVersion.SG3Format => "SG3",
            SGFileVersion.SG3FormatWithAlphaMask => "SG3 · Alpha",
            _ => version.ToString()
        };
    }
}
