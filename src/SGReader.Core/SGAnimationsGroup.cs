using System.Collections.Generic;

namespace SGReader.Core
{
    public class SGAnimationsGroup
    {
        private readonly List<SGAnimation> _animations;

        public SGAnimationsGroup(int orientations, int spritesByAnimation, List<SGAnimation> animations)
        {
            Orientations = orientations;
            SpritesByAnimation = spritesByAnimation;
            _animations = animations;
        }

        public int Orientations { get; }
        
        public int SpritesByAnimation { get; }

        public IReadOnlyList<SGAnimation> Animations => _animations;


    }
}