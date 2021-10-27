using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SGReader
{
    /// <summary>
    /// Logique d'interaction pour SGFileView.xaml
    /// </summary>
    public partial class SGFileView : UserControl
    {
        public SGFileView()
        {
            InitializeComponent();
        }

        private bool _isDataContextChanging = false;

        private void ImagesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isDataContextChanging) return;

            var selectedImages = (DataContext as SGFileViewModel)?.SelectedImages;
            if (selectedImages != null)
            {
                selectedImages.Clear();
                foreach (var image in (e.Source as ListView).SelectedItems.Cast<SGImageViewModel>())
                {
                    selectedImages.Add(image);
                }
            }
        }

        private void ImagesListView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _isDataContextChanging = true;
               var selectedImages = (DataContext as SGFileViewModel)?.SelectedImages;
            if (selectedImages != null)
            {
                (sender as ListView).SelectedItems.Clear();
                foreach (var image in selectedImages)
                {
                    (sender as ListView).SelectedItems.Add(image);
                }
            }

            _isDataContextChanging = false;

        }
    }
}
