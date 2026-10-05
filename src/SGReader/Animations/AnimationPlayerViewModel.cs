using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace SGReader.Animations
{
    public partial class AnimationPlayerViewModel : ObservableObject, IDisposable
    {
        public double MinimumFrame => 0.01;
        public double MaximumFrame => 0.25;
        public double TickFrequency => 0.01;

        private readonly DispatcherTimer _timer;
        private DateTime _start = DateTime.Now;
        private IReadOnlyList<SGImageViewModel> _playbackSprites = Array.Empty<SGImageViewModel>();
        private int _frameIndex;

        [ObservableProperty]
        private double _frame = 0.08;

        [ObservableProperty]
        private bool _isPlaying = true;

        [ObservableProperty]
        private SGAnimationViewModel _animation;

        [ObservableProperty]
        private BitmapImage _currentBitmap;

        [ObservableProperty]
        private double _layoutWidth;

        [ObservableProperty]
        private double _layoutHeight;

        [ObservableProperty]
        private double _drawX;

        [ObservableProperty]
        private double _drawY;

        public AnimationPlayerViewModel()
        {
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(33), DispatcherPriority.Render, TimerCallback, App.Current.Dispatcher);
            _timer.Start();
        }

        private void TimerCallback(object sender, EventArgs e)
        {
            if (!IsPlaying || _playbackSprites.Count <= 1 || Frame <= 0)
                return;

            var elapsed = (DateTime.Now - _start).TotalSeconds;
            var index = (int)(elapsed / Frame) % _playbackSprites.Count;
            if (index == _frameIndex)
                return;

            ShowFrame(index);
        }

        public string StatusLabel => Animation == null
            ? "Select an animation"
            : IsPlaying ? "Playing" : "Paused";

        partial void OnAnimationChanged(SGAnimationViewModel value)
        {
            _playbackSprites = value?.Sprites ?? Array.Empty<SGImageViewModel>();
            _start = DateTime.Now;
            ShowFrame(0);
            OnPropertyChanged(nameof(StatusLabel));
            SaveFrameCommand.NotifyCanExecuteChanged();
        }

        private void ShowFrame(int index)
        {
            _frameIndex = index;
            var sprite = _playbackSprites.Count > 0
                ? _playbackSprites[Math.Clamp(index, 0, _playbackSprites.Count - 1)]
                : null;

            CurrentBitmap = sprite?.Bitmap;
            LayoutWidth = sprite?.LayoutWidth ?? 0;
            LayoutHeight = sprite?.LayoutHeight ?? 0;
            DrawX = sprite?.DrawX ?? 0;
            DrawY = sprite?.DrawY ?? 0;
            SaveFrameCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void TogglePlay()
        {
            IsPlaying = !IsPlaying;
            if (IsPlaying)
                _start = DateTime.Now;
        }

        [RelayCommand(CanExecute = nameof(CanSaveFrame))]
        private void SaveFrame()
        {
            if (CurrentBitmap == null)
                return;

            var sprite = _playbackSprites.Count > 0
                ? _playbackSprites[Math.Clamp(_frameIndex, 0, _playbackSprites.Count - 1)]
                : null;

            var dlg = new SaveFileDialog
            {
                Filter = "PNG image|*.png",
                FileName = $"{Animation?.Title ?? "frame"}_{(sprite?.Id ?? 0):D5}.png",
                DefaultExt = ".png"
            };

            if (dlg.ShowDialog() != true)
                return;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(CurrentBitmap));
            using var stream = File.Create(dlg.FileName);
            encoder.Save(stream);
        }

        private bool CanSaveFrame() => CurrentBitmap != null;

        partial void OnIsPlayingChanged(bool value)
        {
            OnPropertyChanged(nameof(StatusLabel));
            SaveFrameCommand.NotifyCanExecuteChanged();
        }

        public void Dispose()
        {
            _timer.Stop();
        }
    }
}
