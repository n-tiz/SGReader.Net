using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace SGReader.Animations
{
    public class AnimationPlayerViewModel : ViewModelBase
    {
        private readonly ObservableCollection<SGImageViewModel> _sprites;
        public double MinimumFrame => 0.01;
        public double MaximumFrame => 0.1;
        public double TickFrequency => 0.01;

        private double _frame = 0.06;
        private DispatcherTimer _timer;

        public double Frame
        {
            get { return _frame; }
            set
            {
                _frame = value;
                RaisePropertyChanged(nameof(Frame));
            }
        }

        private bool _isPlaying = true;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying == value) return;
                _isPlaying = value;
                RaisePropertyChanged(nameof(IsPlaying));
            }
        }

        public AnimationPlayerViewModel(ObservableCollection<SGImageViewModel> sprites)
        {
            _sprites = sprites;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(60), DispatcherPriority.Render, TimerCallback, App.Current.Dispatcher);
            _timer.Start();
            _start = DateTime.Now;
        }

        private void TimerCallback(object sender, EventArgs e)
        {
        //    _currentFrameIndex++;
        //    if (_currentFrameIndex >= Animation.Frames.Count)
        //    {
        //        _currentFrameIndex = 0;
        //    }

        //    Frame = Animation.Frames.ElementAt(_currentFrameIndex);
        //}

        //void timer_Tick(object sender, EventArgs e)
        //{
            if (IsPlaying)
                RaisePropertyChanged(nameof(CurrentSprite));
        }

        private DateTime _start;

        public SGImageViewModel CurrentSprite
        {
            get
            {
                var count = _sprites.Count;
                if (count == 0) return null;
                double fullTime = count * Frame;
                var elapsedTime = (DateTime.Now - _start).TotalSeconds % fullTime;
                var index = (int)(count * (elapsedTime / fullTime));
                return _sprites.ElementAt(index);
            }
        }

        private ICommand _playCommand;

        public ICommand PlayCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayCommandExecute, PlayCommandCanExecute));

        private bool PlayCommandCanExecute()
        {
            return !IsPlaying;
        }

        private void PlayCommandExecute()
        {
            IsPlaying = true;
        }

        private ICommand _pauseCommand;


        public ICommand PauseCommand => _pauseCommand ?? (_pauseCommand = new RelayCommand(PauseCommandExecute, PauseCommandCanExecute));


        private bool PauseCommandCanExecute()
        {
            return IsPlaying;
        }

        private void PauseCommandExecute()
        {
            IsPlaying = false;
        }
    }
}