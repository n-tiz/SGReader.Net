using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void ImagesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
    }
}
