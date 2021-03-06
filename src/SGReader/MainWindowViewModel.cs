using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using SGReader.Animations;
using SGReader.Core;
using SGReader.Helpers;

namespace SGReader
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        private SGFileViewModel _selectedSGFile;

        public SGFileViewModel SelectedSGFile
        {
            get { return _selectedSGFile; }
            set
            {
                if (_selectedSGFile == value) return;
                _selectedSGFile = value;
                RaisePropertyChanged();
            }
        }

        #endregion  Properties

        #region Commands

        #region Open command

        private ICommand _openCommand;

        public ICommand OpenCommand => _openCommand ?? (_openCommand = new RelayCommand(OpenCommandExecute, OpenCommandCanExecute));

        private bool OpenCommandCanExecute()
        {
            return true;
        }

        private void OpenCommandExecute()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".sg3";
            dlg.Filter = "SG2/3 files|*.sg2;*.sg3";

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                OpenFile(dlg.FileName);
            }
        }

        private void OpenFile(string filePath)
        {
            var sgFile = new SGFile(filePath);
            PopupHelper.WaitUntil(() => sgFile.Load(), "Please wait");
            LoadedFiles.Add(new SGFileViewModel(sgFile));
        }

        public ObservableCollection<SGFileViewModel> LoadedFiles { get; } = new ObservableCollection<SGFileViewModel>();

        #endregion  Open command

        #region Go to github command

        private ICommand _goToGithubCommand;

        public ICommand GoToGithubCommand => _goToGithubCommand ?? (_goToGithubCommand = new RelayCommand(GoToGithubCommandExecute, GoToGithubCommandCanExecute));

        private bool GoToGithubCommandCanExecute()
        {
            return true;
        }

        private void GoToGithubCommandExecute()
        {
            System.Diagnostics.Process.Start("https://github.com/n-tiz/SGReader.Net");
        }

        #endregion  Go to github command

        #region Close image command

        private ICommand _closeCommand;

        public ICommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand<SGFileViewModel>(CloseCommandExecute, CloseCommandCanExecute));

        private bool CloseCommandCanExecute(SGFileViewModel sgFile)
        {
            return true;
        }

        private void CloseCommandExecute(SGFileViewModel sgFile)
        {
            LoadedFiles.Remove(sgFile);
            sgFile.Dispose();
        }

        #endregion  Close image command

        #endregion  Commands
    }
}