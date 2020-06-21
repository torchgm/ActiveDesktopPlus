using PInvoke;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace ActiveDesktop
{
    public partial class ADPVideoWallpaper : Window
    {
        IntPtr VideoPlayerHandle;
        //MainWindow.DisplayInfoCollection Displays = ((MainWindow)Application.Current.MainWindow).Displays;
        bool IsPlaying;

        // Startup events
        public ADPVideoWallpaper(string path)
        {
            InitializeComponent();
            Show();
            VideoPlayerHandle = new WindowInteropHelper(this).Handle;
            VideoPlayer.Source = new Uri(path);
            VideoPlayer.Play();
            IsPlaying = true;
            worker.RunWorkerAsync();
            worker.DoWork += worker_DoWork;
        }

        // Handles looping because WPF sucks and you can't loop things. Don't ask me why
        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Position = new TimeSpan(0, 0, 0, 0, 1);
            VideoPlayer.Play();
            IsPlaying = true;
        }
        
        // Declaration of the mighty background worker!
        private readonly BackgroundWorker worker = new BackgroundWorker();

        // Background Worker that handles checking for fullscreen and stuff
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<IntPtr> ObscuringList = new List<IntPtr>();
            List<IntPtr> RemoveList = new List<IntPtr>();
            Thread.Sleep(10000);
            while (true)
            {
                Thread.Sleep(1000);
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
                        Corner.X = VideoPlayerRect.Left + 10;
                        Corner.Y = VideoPlayerRect.Top + 10;
                        if (MainWindow.PtInRect(in WindowRect, Corner))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                VideoPlayer.Pause();
                                IsPlaying = false;
                            });
                            ObscuringList.Add(wnd);
                        }
                    }
                    else if (!IsPlaying)
                    {
                        foreach (IntPtr ownd in ObscuringList)
                        {
                            if (wnd == ownd)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    VideoPlayer.Play();
                                    IsPlaying = true;
                                });
                                RemoveList.Add(wnd);

                            }
                        }
                        foreach (IntPtr rwnd in RemoveList)
                        {
                            ObscuringList.Remove(rwnd);
                        }
                        RemoveList.Clear();
                    }
                }
            }
        }
    }
}

                        
