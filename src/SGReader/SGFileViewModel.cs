using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

            // Keep leftover stills that aren't part of an index animation (including underlays).
            var usedImageIds = new HashSet<int>();
            foreach (var group in sgFile.AnimationsGroups)
            {
                foreach (var animation in group.Animations)
                {
                    if (animation.BaseImage != null)
                        usedImageIds.Add(animation.BaseImage.Id);

                    foreach (var image in animation.Images)
                        usedImageIds.Add(image.Id);
                }
            }

            foreach (var bitmap in sgFile.Bitmaps)
            {
                var group = SGAnimationsGroupViewModel.FromBitmap(bitmap, usedImageIds);
                if (group != null)
                    AnimationsGroups.Add(group);
            }

            AnimationGroupCount = AnimationsGroups.Count;
            int indexedGroups = sgFile.AnimationsGroups.Count(group => group.Animations.Count > 0);
            Description = indexedGroups > 0
                ? $"{indexedGroups} anims · {AnimationGroupCount} groups · {ImageCount} images"
                : $"{AnimationGroupCount} bitmaps · {ImageCount} images";

            Thumbnail = PickThumbnail();
            SelectedAnimation = AnimationsGroups.SelectMany(g => g.Animations).FirstOrDefault();
            SyncAnimationSelection(SelectedAnimation);
        }

        public AnimationPlayerViewModel AnimationPlayer => _animationPlayer ??= CreateAnimationPlayer();

        public ObservableCollection<SGAnimationsGroupViewModel> AnimationsGroups { get; } = new ObservableCollection<SGAnimationsGroupViewModel>();

        public BitmapImage Thumbnail { get; }

        public bool HasThumbnail => Thumbnail != null;

        [ObservableProperty]
        private SGAnimationViewModel _selectedAnimation;

        partial void OnSelectedAnimationChanged(SGAnimationViewModel value)
        {
            SyncAnimationSelection(value);
            if (_animationPlayer != null)
                _animationPlayer.Animation = value;
        }

        [RelayCommand]
        private void SelectAnimation(SGAnimationViewModel animation)
        {
            if (animation == null)
                return;

            SelectedAnimation = animation;

            // Always rebuild playback: ObservableProperty skips no-op assigns, and a
            // previously stuck selection can leave the player on the wrong frames.
            var player = AnimationPlayer;
            player.Animation = null;
            player.Animation = animation;
        }

        private void SyncAnimationSelection(SGAnimationViewModel selected)
        {
            foreach (var animation in AnimationsGroups.SelectMany(group => group.Animations))
                animation.IsSelected = ReferenceEquals(animation, selected);
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
