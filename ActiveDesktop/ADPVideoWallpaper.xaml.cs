using IWshRuntimeLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.ComponentModel;
using PInvoke;
using Hardcodet.Wpf.TaskbarNotification;
using System.Net.Http.Headers;
using System.Windows.Controls;

namespace ActiveDesktop
{
    public partial class ADPVideoWallpaper : Window
    {
        IntPtr VideoPlayerHandle;
        MainWindow.DisplayInfoCollection Displays = ((MainWindow)Application.Current.MainWindow).Displays;
        bool IsPlaying;

        public ADPVideoWallpaper(string path)
        {
            InitializeComponent();
            this.Show();
            VideoPlayerHandle = new WindowInteropHelper(this).Handle;
            VideoPlayer.Source = new Uri(path);
            VideoPlayer.Play();
            IsPlaying = true;
            worker.RunWorkerAsync();
            worker.DoWork += worker_DoWork;
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Position = new TimeSpan(0, 0, 0, 0, 1);
            VideoPlayer.Play();
            IsPlaying = true;
        }

        private readonly BackgroundWorker worker = new BackgroundWorker();

        // Background Worker that handles checking for fullscreen and stuff
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                for (IntPtr wnd = MainWindow.FindWindowExA(IntPtr.Zero, IntPtr.Zero, null, null); wnd != IntPtr.Zero; wnd = MainWindow.FindWindowExA(IntPtr.Zero, wnd, null, null))
                {
                    int cloaked = -1;
                    MainWindow.DwmGetWindowAttribute(wnd, Convert.ToInt32(DwmApi.DWMWINDOWATTRIBUTE.DWMWA_CLOAKED), out cloaked, Marshal.SizeOf(typeof(bool)));
                    if (MainWindow.IsZoomed(wnd) && MainWindow.IsWindowVisible(wnd) && cloaked <= 0)
                    {
                        MainWindow.RECT WindowRect;
                        MainWindow.RECT VideoPlayerRect;
                        MainWindow.GetWindowRect(wnd, out WindowRect);
                        MainWindow.GetWindowRect(VideoPlayerHandle, out VideoPlayerRect);
                        MainWindow.POINT Corner = new MainWindow.POINT();
                        Corner.X = VideoPlayerRect.Left + 100;
                        Corner.Y = VideoPlayerRect.Top + 100;
                        if (MainWindow.PtInRect(in WindowRect, Corner))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                VideoPlayer.Pause();
                                IsPlaying = false;
                            });
                        }
                        else if (!IsPlaying)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                VideoPlayer.Play();
                                IsPlaying = true;
                            });
                        }
                    }
                }
            }
        }
    }
}

                        
