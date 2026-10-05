using System.Windows;
using System.Windows.Controls;

namespace SGReader
{
    public partial class SGFileView : UserControl
    {
        public SGFileView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            GalleryScrollViewer?.ScrollToHome();
            InspectorScrollViewer?.ScrollToHome();
        }
    }
}
