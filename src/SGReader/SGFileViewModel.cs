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
            Description = $"Images : {_sgFile.Images.Count}";

            foreach (var image in sgFile.Images)
            {
                Images.Add(new SGImageViewModel(image));
            }
            AnimationPlayer = new AnimationPlayerViewModel(SelectedImages);
        }

        public AnimationPlayerViewModel AnimationPlayer { get; }

        public ObservableCollection<SGImageViewModel> Images { get; } = new ObservableCollection<SGImageViewModel>();
        public ObservableCollection<SGImageViewModel> SelectedImages { get; } = new ObservableCollection<SGImageViewModel>();

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