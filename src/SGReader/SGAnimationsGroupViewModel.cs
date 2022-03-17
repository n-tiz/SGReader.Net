using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using SGReader.Core;

namespace SGReader
{
    public class SGAnimationsGroupViewModel : ViewModelBase
    {
        private readonly SGAnimationsGroup _animationsGroup;

        public IReadOnlyCollection<SGAnimationViewModel> Animations { get; }

        public string Name => "TEST";
        public string Description => $"Orientations: {_animationsGroup.Orientations}";

        public SGAnimationsGroupViewModel(SGAnimationsGroup animationsGroup)
        {
            _animationsGroup = animationsGroup;
            Animations = _animationsGroup.Animations.Select(animation => new SGAnimationViewModel(animation)).ToList();
        }
    }
}