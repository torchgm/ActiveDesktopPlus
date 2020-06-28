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

namespace ActiveDesktop.Views
{
    /// <summary>
    /// Interaction logic for CurrentApps.xaml
    /// </summary>
    public partial class CurrentApps : Page
    {
        public CurrentApps()
        {
            InitializeComponent();
        }

        private void ApplyHwndButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).ApplyHwndButton_Click(null, null);
        }
        private void BorderlessButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).BorderlessButton_Click(null, null);
        }
        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).LockButton_Click(null, null);
        }
        private void FixButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).FixButton_Click(null, null);
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).CloseButton_Click(null, null);
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).SaveButton_Click(null, null);
        }
    }
}
