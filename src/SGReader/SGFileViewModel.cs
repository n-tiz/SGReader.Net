using GalaSoft.MvvmLight;
using SGReader.Core;

namespace SGReader
{
    public class SGFileViewModel : ViewModelBase
    {
        private readonly SGFile _sgFile;

        public SGFileViewModel(SGFile sgFile)
        {
            _sgFile = sgFile;
            Name = _sgFile.Name;
            Description = $"Images : {_sgFile.Images.Count}";
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

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

    }
}