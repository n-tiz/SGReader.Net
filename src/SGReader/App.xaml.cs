using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
}
