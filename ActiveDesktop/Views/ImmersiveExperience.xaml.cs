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
    /// Interaction logic for ImmersiveExperience.xaml
    /// </summary>
    public partial class ImmersiveExperience : Page
    {
        public Brush CurrentColour;
        public string SelectedFile = "";
        MainWindow mw;


        public ImmersiveExperience()
        {
            InitializeComponent();
            mw = (MainWindow)Application.Current.MainWindow;
        }

        // Drag logic

        private void DragAndDropDetector_DragEnter(object sender, DragEventArgs e)
        {
            AddWallpaperIcon.Foreground = new SolidColorBrush((Color)ThemeManager.Current.AccentColor);
        }

        private void DragAndDropDetector_DragLeave(object sender, DragEventArgs e)
        {
            AddWallpaperIcon.Foreground = CurrentColour;
        }

        private void DragAndDropDetector_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);


                
                if (files.Length >= 2 || !(files[0].Substring(files[0].Length - 3) == "exe" || files[0].Substring(files[0].Length - 3) == "mp4"))
                {
                    if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                    {
                        AddWallpaperIcon.Foreground = new SolidColorBrush(Colors.Yellow);
                    }
                    else
                    {
                        AddWallpaperIcon.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }
                else
                {
                    AddWallpaperIcon.Foreground = new SolidColorBrush(Colors.ForestGreen);
                    SelectedFile = files[0];
                    ContinueButton.IsEnabled = true;
                }

            }
            else
            {
                AddWallpaperIcon.Foreground = CurrentColour;
            }
        }

        // Buddon

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {

            
            if (SelectedFile.Substring(SelectedFile.Length - 3) == "exe")
            {
                mw.SavedAppsPage.CmdBox.Text = SelectedFile;
            }
            else if (SelectedFile.Substring(SelectedFile.Length - 3) == "mp4")
            {
                mw.SavedAppsPage.CmdBox.Text = "MEDIA";
                mw.SavedAppsPage.FlagBox.Text = SelectedFile;
            }
            mw.ContentFrame.Navigate(mw.ImmersiveMonitorPage);
            mw.ImmersiveMonitorPage.SetupMonitors();
        }
    }
}
