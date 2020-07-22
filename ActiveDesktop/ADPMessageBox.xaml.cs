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

namespace ActiveDesktop
{
    /// <summary>
    /// Interaction logic for ADPMessageBox.xaml
    /// </summary>
    public partial class ADPMessageBox : Window
    {
        public ADPMessageBox()
        {
            InitializeComponent();
            ((MainWindow)Application.Current.MainWindow).IsEnabled = false;
        }

        private void MessageOKButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).IsEnabled = true;
            Close();
        }
    }
}
