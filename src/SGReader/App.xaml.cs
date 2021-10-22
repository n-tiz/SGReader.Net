using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace SGReader
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            MainWindowViewModel = new MainWindowViewModel();
        }

        public MainWindowViewModel MainWindowViewModel { get; }

        public new static App Current => (App) Application.Current;
    }

    public class MainWindowViewModel : ViewModelBase
    {
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
            
        }

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
            
        }

        #endregion  Go to github command

        #endregion  Commands
    }
}
