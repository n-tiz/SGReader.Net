using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SGReader
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplicationThemeManager.Apply(this);
            DataContext = App.Current.MainWindowViewModel;
        }
    }
}
