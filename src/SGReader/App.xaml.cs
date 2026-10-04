using System.Windows;

namespace SGReader
{
    public partial class App : Application
    {
        public App()
        {
            MainWindowViewModel = new MainWindowViewModel();
        }

        public MainWindowViewModel MainWindowViewModel { get; }

        public new static App Current => (App)Application.Current;
    }
}
