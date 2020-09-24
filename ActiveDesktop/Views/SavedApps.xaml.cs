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
using Microsoft.Win32;

namespace ActiveDesktop.Views
{
    /// <summary>
    /// Interaction logic for SavedApps.xaml
    /// </summary>
    public partial class SavedApps : Page
    {
        MainWindow mw = (MainWindow)Application.Current.MainWindow;
        public SavedApps()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            mw.AddButton_Click(null, null);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            mw.RemoveButton_Click(null, null);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            mw.TestButton_Click(null, null);
        }

        public void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            mw.WriteButton_Click(null, null);
        }

        private void MonitorSelectButton_Click(object sender, RoutedEventArgs e)
        {
            mw.MonitorSelectButton_Click(null, null);
        }

        private void CmdBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CmdBox.Text == "MEDIA")
            {
                CmdBox.IsEnabled = false;
                FlagBox.Text = "Path to Video";
                FlagBox.ToolTip = "The file path to the video you wish to use as a wallpaper";
                MediaButton.Content = "    Use \nWebsite";
                MediaButton.Content = "    Use \nWebsite";
                FileSelectButton.Visibility = Visibility.Hidden;
                VideoSelectButton.Visibility = Visibility.Visible;
                FileOpenIcon.Visibility = Visibility.Hidden;
                VideoOpenIcon.Visibility = Visibility.Visible;
                CmdBox.Width = 236;
                FlagBox.Width = 202;
            }
            else if (CmdBox.Text == "WEB")
            {
                CmdBox.IsEnabled = false;
                FlagBox.Text = "URL";
                FlagBox.ToolTip = "The URL for the website you wish to use as a wallpaper";
                MediaButton.Content = "    Use \nProgram";
                MediaButton.Content = "    Use \nProgram";
                FileSelectButton.Visibility = Visibility.Hidden;
                VideoSelectButton.Visibility = Visibility.Hidden;
                FileOpenIcon.Visibility = Visibility.Hidden;
                VideoOpenIcon.Visibility = Visibility.Hidden;
                CmdBox.Width = 236;
                FlagBox.Width = 236;
            } else
            {
                FileSelectButton.Visibility = Visibility.Visible;
                VideoSelectButton.Visibility = Visibility.Hidden;
                FileOpenIcon.Visibility = Visibility.Visible;
                VideoOpenIcon.Visibility = Visibility.Hidden;
            }
            mw.CmdBox_LostFocus(null, null);

        }

        private void ResetMonitorSelectButton(object sender, RoutedEventArgs e)
        {
            mw.ResetMonitorSelectButton(null, null);
        }

        private void MediaButton_Click(object sender, RoutedEventArgs e)
        {
            if (CmdBox.Text != "MEDIA" && CmdBox.Text != "WEB")
            {
                CmdBox.Text = "MEDIA";
                CmdBox_LostFocus(null, null);

            }
            else if (CmdBox.Text != "WEB")
            {
                CmdBox.Text = "WEB";
                CmdBox_LostFocus(null, null);

            } else 
            {
                CmdBox.Text = "Command Line";

                CmdBox.Width = 202;
                FlagBox.Width = 236;
                CmdBox.IsEnabled = true;
                FlagBox.Text = "Flags";
                FlagBox.ToolTip = "Any paths, flags or command-line-switches you wish to apply to the program at startup";
                CmdBox_LostFocus(null, null);
                MediaButton.Content = "  Use \nVideo";
            }
        }

        private void FileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dl = new OpenFileDialog();
            dl.Filter = dl.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dl.ShowDialog();
            dl.FilterIndex = 2;
            dl.RestoreDirectory = true;
            if (dl.FileName != "")
            {
                CmdBox.Text = dl.FileName;
            }
        }

        private void VideoSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dl = new OpenFileDialog();
            dl.Filter = dl.Filter = "Video files (*.mp4)|*.mp4|All files (*.*)|*.*";
            dl.ShowDialog();
            dl.FilterIndex = 2;
            dl.RestoreDirectory = true;
            if (dl.FileName != "")
            {
                FlagBox.Text = dl.FileName;
            }
        }

        private void CmdBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CmdBox.Text == "MEDIA" || CmdBox.Text == "Command Line")
            {
                CmdBox.Text = "";
                FlagBox.Text = "Flags";
            }
        }

        private void FlagBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FlagBox.Text == "")
            {
                if (CmdBox.Text == "MEDIA")
                {
                    FlagBox.Text = "Path to Video";
                }
                else if (CmdBox.Text == "WEB")
                {
                    FlagBox.Text = "URL";
                } else
                {
                    FlagBox.Text = "Flags";
                }
            }
        }

        private void FlagBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FlagBox.Text == "Path to Video" || FlagBox.Text == "Flags")
            {
                FlagBox.Text = "";
            }
        }

        private void XBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (XBox.Text == "")
            {
                XBox.Text = "X";
            }
        }

        private void XBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (XBox.Text == "X")
            {
                XBox.Text = "";
            }
        }

        private void YBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (YBox.Text == "")
            {
                YBox.Text = "Y";
            }
        }

        private void YBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (YBox.Text == "Y")
            {
                YBox.Text = "";
            }
        }

        private void WidthBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WidthBox.Text == "")
            {
                WidthBox.Text = "Width";
            }
        }

        private void WidthBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (WidthBox.Text == "Width")
            {
                WidthBox.Text = "";
            }
        }

        private void HeightBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HeightBox.Text == "")
            {
                HeightBox.Text = "Height";
            }
        }

        private void HeightBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (HeightBox.Text == "Height")
            {
                HeightBox.Text = "";
            }
        }

        private void NameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NameBox.Text == "")
            {
                NameBox.Text = "Friendly Name";
            }
        }

        private void NameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NameBox.Text == "Friendly Name")
            {
                NameBox.Text = "";
            }
        }

        private void TimeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TimeBox.Text == "")
            {
                TimeBox.Text = "Wait Time";
            }
        }

        private void TimeBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TimeBox.Text == "Wait Time")
            {
                TimeBox.Text = "";
            }
        }
    }
}
