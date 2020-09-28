using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using System;
using System.Windows;

namespace ActiveDesktop
{
    /// <summary>
    /// Interaction logic for ADPWebWallpaper.xaml
    /// </summary>
    public partial class ADPWebWallpaper : Window
    {
        string navto = "";
        public ADPWebWallpaper(string url)
        {
            navto = url;
            InitializeComponent();
            WebView.Loaded += WebView_Loaded;
            System.Diagnostics.Debug.WriteLine("Window created");
            Show();
        }

        private void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            Potato();
        }

        private void Potato()
        {
            System.Diagnostics.Debug.WriteLine("Potatoing to " + navto);
            WebView.Navigate(navto);
            System.Diagnostics.Debug.WriteLine("Potatoed");
        }
    }
}