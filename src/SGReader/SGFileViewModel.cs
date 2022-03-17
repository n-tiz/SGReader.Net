using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using SGReader.Animations;
using SGReader.Core;

namespace SGReader
{
    public class SGFileViewModel : ViewModelBase, IDisposable
    {
        private readonly SGFile _sgFile;

        public SGFileViewModel(SGFile sgFile)
        {
            _sgFile = sgFile;
            Name = _sgFile.Name;
            Description = $"Animations : {_sgFile.AnimationsGroups.Count}";

            foreach (var animation in sgFile.AnimationsGroups)
            {
                AnimationsGroups.Add(new SGAnimationsGroupViewModel(animation));
            }
            AnimationPlayer = new AnimationPlayerViewModel();
        }

        public AnimationPlayerViewModel AnimationPlayer { get; }

        public ObservableCollection<SGAnimationsGroupViewModel> AnimationsGroups { get; } = new ObservableCollection<SGAnimationsGroupViewModel>();
        private SGAnimationViewModel _selectedAnimation;

        public SGAnimationViewModel SelectedAnimation
        {
            get { return _selectedAnimation; }
            set
            {
                if (_selectedAnimation == value) return;
                _selectedAnimation = value;
                AnimationPlayer.Animation = SelectedAnimation;
                RaisePropertyChanged();
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged();
            }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                RaisePropertyChanged();
            }
        }

        public void Dispose()
        {
            _sgFile?.Dispose();
        }
    }
}