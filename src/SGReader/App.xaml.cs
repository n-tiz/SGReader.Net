using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SGReader
{
    public partial class App : Application
    {
        public App()
        {
            MainWindowViewModel = new MainWindowViewModel();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica, updateAccent: true);
            base.OnStartup(e);
        }

        public MainWindowViewModel MainWindowViewModel { get; }

        public new static App Current => (App)Application.Current;
    }
}
