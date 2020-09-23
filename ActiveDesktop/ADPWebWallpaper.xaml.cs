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
using System.Windows.Shapes;
using System.Threading;

namespace ActiveDesktop
{
    /// <summary>
    /// Interaction logic for ADPWebWallpaper.xaml
    /// </summary>
    public partial class ADPWebWallpaper : Window
    {
        MainWindow mw = (MainWindow)Application.Current.MainWindow;
        string LogID;
        public ADPWebWallpaper(string url)
        {
            InitializeComponent();
            Show();
            Random rnd = new Random();
            LogID = rnd.Next(9).ToString() + rnd.Next(9).ToString() + rnd.Next(9).ToString() + rnd.Next(9).ToString() + rnd.Next(9).ToString();
            mw.LogEntry("[WEB] [ADPWebWallpaper" + LogID + "] WebView started");
            LoadPage(url);
            mw.LogEntry("[WEB] [ADPWebWallpaper" + LogID + "] WebView loaded: " + url);
        }

        private void LoadPage(string navto)
        {
            //Thread.Sleep(100);
            WebView.Navigate(navto);
        }
    }
}
