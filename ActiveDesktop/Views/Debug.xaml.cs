using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ActiveDesktop.Views
{
    /// <summary>
    /// Interaction logic for Debug.xaml
    /// </summary>
    public partial class Debug : Page
    {
        public Debug()
        {
            InitializeComponent();
        }

        private void DebugRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).DesktopHandle = ((MainWindow)Application.Current.MainWindow).GetDesktopWindowHandle();
            ((MainWindow)Application.Current.MainWindow).Displays = ((MainWindow)Application.Current.MainWindow).GetDisplays();
            ((MainWindow)Application.Current.MainWindow).RefreshLists();
            ((MainWindow)Application.Current.MainWindow).DebugRefreshEvent();
        }

        private void OpenLogsButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(System.IO.Path.Combine(((MainWindow)Application.Current.MainWindow).LocalFolder, "adp.log"));
        }
    }
}
