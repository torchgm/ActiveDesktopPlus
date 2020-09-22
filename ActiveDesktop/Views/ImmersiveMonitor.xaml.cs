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
    /// Interaction logic for ImmersiveMonitor.xaml
    /// </summary>
    public partial class ImmersiveMonitor : Page
    {

        Brush UBGBrush;
        Brush UOLBrush;
        Brush SBGBrush;
        Brush SOLBrush;
        MainWindow mw;
        int WorkWidth;
        int WorkHeight;
        public ImmersiveMonitor()
        {
            InitializeComponent();
            UBGBrush = IntRect0.Background;
            UOLBrush = IntRect0.BorderBrush;
            SBGBrush = new SolidColorBrush((Color)ThemeManager.Current.AccentColor);
            SOLBrush = new SolidColorBrush(Colors.White);
        }

        public void SetupMonitors()
        {
            mw = (MainWindow)Application.Current.MainWindow;
            if (mw.Displays.Count <= 4)
            {
                WorkWidth = Convert.ToInt32(MainWindow.GetWindowSize(mw.DesktopHandle).Width);
                WorkHeight = Convert.ToInt32(MainWindow.GetWindowSize(mw.DesktopHandle).Height);
                double ScaleFactor = 1;

                if (WorkWidth >= WorkHeight)
                {
                    ScaleFactor = Math.Round((double)(WorkWidth / (double)231)); 
                }
                else // Hooray for unnecessary casting!!
                {
                    ScaleFactor = Math.Round((double)(WorkHeight / (double)231));
                }
                
                Workspace.Width = WorkWidth/ScaleFactor;
                Workspace.Height = WorkHeight/ScaleFactor;

                Workspace.Margin = new Thickness((231 - Workspace.Width) / 2, (231 - Workspace.Height) / 2, ((231 - Workspace.Width) / 2) - 1, ((231 - Workspace.Height) / 2) - 1);

                for (int i = 0; i < mw.Displays.Count; i++)
                {
                    if (i == 0)
                    {

                        MonitorRect0.Width = Convert.ToDouble(mw.Displays[i].ScreenWidth) / ScaleFactor;
                        MonitorRect0.Height = Convert.ToDouble(mw.Displays[i].ScreenHeight) / ScaleFactor;
                        MonitorRect0.Margin = new Thickness(Convert.ToDouble(mw.Displays[i].Left + mw.GlobalXCorrection) / ScaleFactor, Convert.ToDouble(mw.Displays[i].Top + mw.GlobalYCorrection) / ScaleFactor, 0, 0);
                        MonitorRect0.Visibility = Visibility.Visible;
                    }
                    else if (i == 1)
                    {
                        MonitorRect1.Width = Convert.ToDouble(mw.Displays[i].ScreenWidth) / ScaleFactor;
                        MonitorRect1.Height = Convert.ToDouble(mw.Displays[i].ScreenHeight) / ScaleFactor;
                        MonitorRect1.Margin = new Thickness(Convert.ToDouble(mw.Displays[i].Left + mw.GlobalXCorrection) / ScaleFactor, Convert.ToDouble(mw.Displays[i].Top + mw.GlobalYCorrection) / ScaleFactor, 0, 0);
                        MonitorRect1.Visibility = Visibility.Visible;
                    }
                    else if (i == 2)
                    {
                        MonitorRect2.Width = Convert.ToDouble(mw.Displays[i].ScreenWidth) / ScaleFactor;
                        MonitorRect2.Height = Convert.ToDouble(mw.Displays[i].ScreenHeight) / ScaleFactor;
                        MonitorRect2.Margin = new Thickness(Convert.ToDouble(mw.Displays[i].Left + mw.GlobalXCorrection) / ScaleFactor, Convert.ToDouble(mw.Displays[i].Top + mw.GlobalYCorrection) / ScaleFactor, 0, 0);
                        MonitorRect2.Visibility = Visibility.Visible;
                    }
                    else if (i == 3)
                    {
                        MonitorRect3.Width = Convert.ToDouble(mw.Displays[i].ScreenWidth) / ScaleFactor;
                        MonitorRect3.Height = Convert.ToDouble(mw.Displays[i].ScreenHeight) / ScaleFactor;
                        MonitorRect3.Margin = new Thickness(Convert.ToDouble(mw.Displays[i].Left + mw.GlobalXCorrection) / ScaleFactor, Convert.ToDouble(mw.Displays[i].Top + mw.GlobalYCorrection) / ScaleFactor, 0, 0);
                        MonitorRect3.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void IntRect0_Click(object sender, RoutedEventArgs e)
        {
            IntRect0.Background = SBGBrush;
            IntRect0.BorderBrush = SOLBrush;
            IntRect1.Background = UBGBrush;
            IntRect1.BorderBrush = UOLBrush;
            IntRect2.Background = UBGBrush;
            IntRect2.BorderBrush = UOLBrush;
            IntRect3.Background = UBGBrush;
            IntRect3.BorderBrush = UOLBrush;
            mw.SelectedDisplay = -1;
            mw.MonitorSelectButton_Click(null, null);
            ContinueButton.IsEnabled = true;
        }

        private void IntRect1_Click(object sender, RoutedEventArgs e)
        {
            IntRect0.Background = UBGBrush;
            IntRect0.BorderBrush = UOLBrush;
            IntRect1.Background = SBGBrush;
            IntRect1.BorderBrush = SOLBrush;
            IntRect2.Background = UBGBrush;
            IntRect2.BorderBrush = UOLBrush;
            IntRect3.Background = UBGBrush;
            IntRect3.BorderBrush = UOLBrush;
            mw.SelectedDisplay = 0;
            mw.MonitorSelectButton_Click(null, null);
            ContinueButton.IsEnabled = true;
        }

        private void IntRect2_Click(object sender, RoutedEventArgs e)
        {
            IntRect0.Background = UBGBrush;
            IntRect0.BorderBrush = UOLBrush;
            IntRect1.Background = UBGBrush;
            IntRect1.BorderBrush = UOLBrush;
            IntRect2.Background = SBGBrush;
            IntRect2.BorderBrush = SOLBrush;
            IntRect3.Background = UBGBrush;
            IntRect3.BorderBrush = UOLBrush;
            mw.SelectedDisplay = 1;
            mw.MonitorSelectButton_Click(null, null);
            ContinueButton.IsEnabled = true;
        }

        private void IntRect3_Click(object sender, RoutedEventArgs e)
        {
            IntRect0.Background = UBGBrush;
            IntRect0.BorderBrush = UOLBrush;
            IntRect1.Background = UBGBrush;
            IntRect1.BorderBrush = UOLBrush;
            IntRect2.Background = UBGBrush;
            IntRect2.BorderBrush = UOLBrush;
            IntRect3.Background = SBGBrush;
            IntRect3.BorderBrush = SOLBrush;
            mw.SelectedDisplay = 2;
            mw.MonitorSelectButton_Click(null, null);
            ContinueButton.IsEnabled = true;
        }

        public void Unclick()
        {
            IntRect0.Background = UBGBrush;
            IntRect0.BorderBrush = UOLBrush;
            IntRect1.Background = UBGBrush;
            IntRect1.BorderBrush = UOLBrush;
            IntRect2.Background = UBGBrush;
            IntRect2.BorderBrush = UOLBrush;
            IntRect3.Background = UBGBrush;
            IntRect3.BorderBrush = UOLBrush;
            mw.SelectedDisplay = -1;
            mw.CmdBox_LostFocus(null, null);
            ContinueButton.IsEnabled = false;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            mw.SavedAppsPage.NameBox.Text = "Wallpaper (Monitor " + (mw.SelectedDisplay + 2).ToString() + ")";
            mw.SavedAppsPage.AutostartCheckBox.IsChecked = true;
            mw.ContentFrame.Navigate(mw.ImmersiveFinalisePage);
            if (mw.ImmersiveExperiencePage.SelectedFile.Contains(".exe") && !mw.ImmersiveExperiencePage.SelectedFile.Contains(".mp4") && WorkWidth + WorkHeight > 0)
            {
                mw.ImmersiveFinalisePage.WarningBlock.Visibility = Visibility.Visible;
            }
            else
            {
                mw.ImmersiveFinalisePage.WarningBlock.Visibility = Visibility.Hidden;
            }
        }
    }
}
