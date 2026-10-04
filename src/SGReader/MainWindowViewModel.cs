using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SGReader.Core;

namespace SGReader
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private SGFileViewModel _selectedSGFile;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenCommand))]
        private bool _isLoading;

        [ObservableProperty]
        private string _loadingMessage = "Loading…";

        public ObservableCollection<SGFileViewModel> LoadedFiles { get; } = new ObservableCollection<SGFileViewModel>();

        public bool HasFiles => LoadedFiles.Count > 0;

        [RelayCommand(CanExecute = nameof(CanOpen))]
        private async Task Open()
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".sg3",
                Filter = "SG2/3 files|*.sg2;*.sg3",
                Multiselect = true
            };

            if (dlg.ShowDialog() != true)
                return;

            IsLoading = true;
            SGFileViewModel lastOpened = null;

            try
            {
                foreach (var path in dlg.FileNames)
                {
                    var fileName = Path.GetFileName(path);
                    LoadingMessage = $"Loading {fileName}…";

                    var vm = await Task.Run(() =>
                    {
                        var file = new SGFile(path);
                        file.Load();
                        return new SGFileViewModel(file);
                    }).ConfigureAwait(true);

                    LoadedFiles.Add(vm);
                    lastOpened = vm;
                    OnPropertyChanged(nameof(HasFiles));
                }

                if (lastOpened != null)
                    SelectedSGFile = lastOpened;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Failed to open SG file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = "Loading…";
            }
        }

        private bool CanOpen() => !IsLoading;

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
        private void CloseFile(SGFileViewModel sgFile)
        {
            if (sgFile == null || IsLoading)
                return;

            var index = LoadedFiles.IndexOf(sgFile);
            LoadedFiles.Remove(sgFile);
            sgFile.Dispose();

            if (ReferenceEquals(SelectedSGFile, sgFile))
            {
                if (LoadedFiles.Count == 0)
                    SelectedSGFile = null;
                else
                    SelectedSGFile = LoadedFiles[Math.Clamp(index, 0, LoadedFiles.Count - 1)];
            }

            OnPropertyChanged(nameof(HasFiles));
        }
    }
}
