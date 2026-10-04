using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SGReader.Core;
using SGReader.Helpers;

namespace SGReader
{
    public class SGAnimationViewModel : ObservableObject
    {
        private readonly SGAnimation _animation;
        private IReadOnlyList<SGImageViewModel> _sprites;

        public IReadOnlyList<SGImageViewModel> Sprites => _sprites ??= BuildSprites();

        public SGImageViewModel Preview => Sprites.Count > 0 ? Sprites[0] : null;

        public int Count => _animation.Images.Count;

        public string Title { get; }
        public string Description => $"{Count} frames";
        public string FullDescription => string.Join(" → ", _animation.Images.Select(s => s.Id));
        public string BitmapName => _animation.Images.FirstOrDefault()?.Parent?.FileName ?? "—";
        public string SizeLabel => Preview == null ? "—" : Preview.Description;
        public string TypeLabel { get; }

        public SGAnimationViewModel(SGAnimation animation)
        {
            _animation = animation;
            var first = _animation.Images.FirstOrDefault();
            Title = string.IsNullOrWhiteSpace(_animation.Name)
                ? (first?.Parent?.FileName ?? $"Animation {first?.Id}")
                : _animation.Name;
            TypeLabel = SGImageTypeHelper.FormatTypes(_animation.Images.Select(i => i.Type));
        }

        private IReadOnlyList<SGImageViewModel> BuildSprites()
        {
            return _animation.Images.Select(i => new SGImageViewModel(i)).ToList();
        }
    }
}
