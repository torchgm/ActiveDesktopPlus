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
using ModernWpf;


namespace ActiveDesktop.Views
{
    /// <summary>
    /// Interaction logic for ImmersiveFinalise.xaml
    /// </summary>
    public partial class ImmersiveFinalise : Page
    {
        MainWindow mw;

        public ImmersiveFinalise()
        {
            InitializeComponent();
            mw = (MainWindow)Application.Current.MainWindow;
        }

        private void FinaliseButton_Click(object sender, RoutedEventArgs e)
        {
            mw.SavedAppsPage.AutostartCheckBox.IsChecked = true;
            mw.AddButton_Click(null, null);
            mw.SavedAppsPage.WriteButton_Click(null, null);
            mw.SavedAppsPage.SavedListBox.SelectedIndex = (mw.SavedAppsPage.SavedListBox.Items.Count - 1);
            mw.TestButton_Click(null, null);
            mw.CmdBox_LostFocus(null, null);
            TidyUp();
        }

        private void AdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            mw.ContentFrame.Navigate(mw.SavedAppsPage);
            mw.CmdBox_LostFocus(null, null);
            TidyUp();
        }

        private void TidyUp()
        {
            mw.SavedAppsPage.AutostartCheckBox.IsChecked = false;
            mw.ImmersiveExperiencePage.SelectedFile = "";
            mw.ImmersiveExperiencePage.FileValidLabel.Content = "";
            mw.ImmersiveMonitorPage.Unclick();
            mw.ImmersiveExperiencePage.ContinueButton.IsEnabled = false;
            mw.ImmersiveExperiencePage.AddWallpaperIcon.Foreground = new SolidColorBrush((Color)ThemeManager.Current.AccentColor);
        }
    }
}
