using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SGReader.Core;

namespace SGReader
{
    public class SGAnimationViewModel : ObservableObject
    {
        private readonly SGAnimation _animation;

        public IReadOnlyCollection<SGImageViewModel> Sprites { get; }
        public SGImageViewModel Preview => Sprites.FirstOrDefault();
        public int Count => Sprites.Count;

        public string Title { get; }
        public string Description => $"{Count} frames";
        public string FullDescription => string.Join(" → ", Sprites.Select(s => s.Id));
        public string BitmapName => Preview?.Group ?? "—";
        public string SizeLabel => Preview == null ? "—" : Preview.Description;

        public SGAnimationViewModel(SGAnimation animation)
        {
            _animation = animation;
            Sprites = animation.Images.Select(i => new SGImageViewModel(i)).ToList();
            Title = string.IsNullOrWhiteSpace(_animation.Name)
                ? (Preview?.Group ?? $"Animation {Preview?.Id}")
                : _animation.Name;
        }
    }
}
