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
    /// Interaction logic for SavedApps.xaml
    /// </summary>
    public partial class SavedApps : Page
    {
        public SavedApps()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).AddButton_Click(null, null);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).RemoveButton_Click(null, null);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).TestButton_Click(null, null);
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).WriteButton_Click(null, null);
        }

        private void MonitorSelectButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MonitorSelectButton_Click(null, null);
        }

        private void CmdBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CmdBox.Text == "MEDIA")
            {
                CmdBox.IsEnabled = false;
                FlagBox.Text = "Path to Video";
                FlagBox.ToolTip = "The file path to the video you wish to use as a wallpaper";
                MediaButton.Content = "    Use \nProgram";
            }
            ((MainWindow)Application.Current.MainWindow).CmdBox_LostFocus(null, null);

        }

        private void ResetMonitorSelectButton(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).ResetMonitorSelectButton(null, null);
        }

        private void MediaButton_Click(object sender, RoutedEventArgs e)
        {
            if (CmdBox.Text != "MEDIA")
            {
                CmdBox.Text = "MEDIA";
                CmdBox_LostFocus(null, null);
                MediaButton.Content = "    Use \nProgram";
            }
            else
            {
                CmdBox.Text = "Command Line";
                CmdBox.IsEnabled = true;
                FlagBox.Text = "Flags";
                FlagBox.ToolTip = "Any paths, flags or command-line-switches you wish to apply to the program at startup";
                CmdBox_LostFocus(null, null);
                MediaButton.Content = "  Use \nVideo";
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
                else
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
