using System;
using System.IO;
using System.Linq;
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
        public double MaximumFrame => 0.1;
        public double TickFrequency => 0.01;

        private readonly DispatcherTimer _timer;
        private DateTime _start = DateTime.Now;

        [ObservableProperty]
        private double _frame = 0.06;

        [ObservableProperty]
        private bool _isPlaying = true;

        [ObservableProperty]
        private SGAnimationViewModel _animation;

        public AnimationPlayerViewModel()
        {
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(33), DispatcherPriority.Render, TimerCallback, App.Current.Dispatcher);
            _timer.Start();
        }

        private void TimerCallback(object sender, EventArgs e)
        {
            if (IsPlaying && Animation != null)
            {
                OnPropertyChanged(nameof(CurrentSprite));
                SaveFrameCommand.NotifyCanExecuteChanged();
            }
        }

        public SGImageViewModel CurrentSprite
        {
            get
            {
                var count = Animation?.Sprites.Count ?? 0;
                if (count == 0)
                    return null;

                double fullTime = count * Frame;
                if (fullTime <= 0)
                    return Animation.Sprites.FirstOrDefault();

                var elapsedTime = (DateTime.Now - _start).TotalSeconds % fullTime;
                var index = (int)(count * (elapsedTime / fullTime));
                return Animation.Sprites.ElementAt(Math.Clamp(index, 0, count - 1));
            }
        }

        public string StatusLabel => Animation == null
            ? "Select an animation"
            : IsPlaying ? "Playing" : "Paused";

        partial void OnAnimationChanged(SGAnimationViewModel value)
        {
            _start = DateTime.Now;
            OnPropertyChanged(nameof(CurrentSprite));
            OnPropertyChanged(nameof(StatusLabel));
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
            var sprite = CurrentSprite;
            if (sprite?.Bitmap == null)
                return;

            var dlg = new SaveFileDialog
            {
                Filter = "PNG image|*.png",
                FileName = $"{Animation?.Title ?? "frame"}_{sprite.Id:D5}.png",
                DefaultExt = ".png"
            };

            if (dlg.ShowDialog() != true)
                return;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(sprite.Bitmap));
            using var stream = File.Create(dlg.FileName);
            encoder.Save(stream);
        }

        private bool CanSaveFrame() => CurrentSprite?.Bitmap != null;

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
