using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SGReader.Core;

namespace SGReader
{
    public class SGAnimationsGroupViewModel : ObservableObject
    {
        private readonly SGAnimationsGroup _animationsGroup;

        public IReadOnlyCollection<SGAnimationViewModel> Animations { get; }

        public string Name { get; }

        public string Description { get; }

        public SGAnimationsGroupViewModel(SGAnimationsGroup animationsGroup)
        {
            _animationsGroup = animationsGroup;
            Animations = _animationsGroup.Animations
                .Select(animation => new SGAnimationViewModel(animation))
                .ToList();

            Name = Animations.FirstOrDefault()?.Title ?? "Animation group";
            Description = $"{_animationsGroup.Orientations} orientations · {_animationsGroup.SpritesByAnimation} frames";
        }
    }
}
