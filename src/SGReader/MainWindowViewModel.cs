using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SGReader.Core;
using SGReader.Helpers;

namespace SGReader
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private SGFileViewModel _selectedSGFile;

        public ObservableCollection<SGFileViewModel> LoadedFiles { get; } = new ObservableCollection<SGFileViewModel>();

        [RelayCommand]
        private void Open()
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".sg3",
                Filter = "SG2/3 files|*.sg2;*.sg3"
            };

            if (dlg.ShowDialog() == true)
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

        [RelayCommand]
        private void GoToGithub()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/n-tiz/SGReader.Net",
                UseShellExecute = true
            });
        }

        [RelayCommand]
        private void Close(SGFileViewModel sgFile)
        {
            if (sgFile == null) return;
            LoadedFiles.Remove(sgFile);
            sgFile.Dispose();
        }
    }
}
