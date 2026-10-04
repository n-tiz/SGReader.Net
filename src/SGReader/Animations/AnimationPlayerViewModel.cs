using System;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SGReader.Animations
{
    public partial class AnimationPlayerViewModel : ObservableObject
    {
        public double MinimumFrame => 0.01;
        public double MaximumFrame => 0.1;
        public double TickFrequency => 0.01;

        private readonly DispatcherTimer _timer;
        private readonly DateTime _start;

        [ObservableProperty]
        private double _frame = 0.06;

        [ObservableProperty]
        private bool _isPlaying = true;

        public AnimationPlayerViewModel()
        {
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(60), DispatcherPriority.Render, TimerCallback, App.Current.Dispatcher);
            _timer.Start();
            _start = DateTime.Now;
        }

        public SGAnimationViewModel Animation { get; set; }

        private void TimerCallback(object sender, EventArgs e)
        {
            if (IsPlaying)
                OnPropertyChanged(nameof(CurrentSprite));
        }

        public SGImageViewModel CurrentSprite
        {
            get
            {
                var count = Animation?.Sprites.Count ?? 0;
                if (count == 0) return null;
                double fullTime = count * Frame;
                var elapsedTime = (DateTime.Now - _start).TotalSeconds % fullTime;
                var index = (int)(count * (elapsedTime / fullTime));
                return Animation.Sprites.ElementAt(index);
            }
        }

        [RelayCommand(CanExecute = nameof(CanPlay))]
        private void Play()
        {
            IsPlaying = true;
        }

        private bool CanPlay() => !IsPlaying;

        [RelayCommand(CanExecute = nameof(CanPause))]
        private void Pause()
        {
            IsPlaying = false;
        }

        private bool CanPause() => IsPlaying;

        partial void OnIsPlayingChanged(bool value)
        {
            PlayCommand.NotifyCanExecuteChanged();
            PauseCommand.NotifyCanExecuteChanged();
        }
    }
}
