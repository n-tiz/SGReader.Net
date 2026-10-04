using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SGReader.Animations;
using SGReader.Core;

namespace SGReader
{
    public partial class SGFileViewModel : ObservableObject, IDisposable
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

        [ObservableProperty]
        private SGAnimationViewModel _selectedAnimation;

        partial void OnSelectedAnimationChanged(SGAnimationViewModel value)
        {
            AnimationPlayer.Animation = value;
        }

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _description;

        public void Dispose()
        {
            _sgFile?.Dispose();
        }
    }
}
