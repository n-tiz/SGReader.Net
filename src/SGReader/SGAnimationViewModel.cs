using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SGReader.Core;

namespace SGReader
{
    public class SGAnimationViewModel : ViewModelBase
    {
        private readonly SGAnimation _animation;
        
        public IReadOnlyCollection<SGImageViewModel> Sprites { get; }
        public SGImageViewModel Preview => Sprites.FirstOrDefault();
        public int Count => Sprites.Count;

        public string Name => _animation.Name;
        public string Description => $"Images: {Count}";

        public SGAnimationViewModel(SGAnimation animation)
        {
            _animation = animation;
            Sprites = animation.Images.Select(i => new SGImageViewModel(i)).ToList();
        }

    }
}