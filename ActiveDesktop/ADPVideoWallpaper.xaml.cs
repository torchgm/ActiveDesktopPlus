using PInvoke;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;

namespace ActiveDesktop
{
    public partial class ADPVideoWallpaper : Window
    {
        bool PauseOnBat = ((MainWindow)System.Windows.Application.Current.MainWindow).PauseOnBattery;
        bool PauseOnMax = ((MainWindow)System.Windows.Application.Current.MainWindow).PauseOnMaximise;
        bool PauseOnBatSave = ((MainWindow)System.Windows.Application.Current.MainWindow).PauseOnBatterySaver;
        string LogID;
        IntPtr VideoPlayerHandle;
        //MainWindow.DisplayInfoCollection Displays = ((MainWindow)Application.Current.MainWindow).Displays;
        bool IsPlaying;

        // Startup events
        public ADPVideoWallpaper(string path)
        {
            InitializeComponent();
            Show();
            Random rnd = new Random();
            LogID = rnd.Next(9).ToString() + rnd.Next(9).ToString() + rnd.Next(9).ToString() + rnd.Next(9).ToString() + rnd.Next(9).ToString();
            ((MainWindow)System.Windows.Application.Current.MainWindow).LogEntry("[ADPVideoPlayer" + LogID + "] Player started");
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
            Dispatcher.Invoke(() =>
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).LogEntry("[ADPVideoPlayer" + LogID + "] BackgroundWorker started");
            });
            List<IntPtr> ObscuringList = new List<IntPtr>();
            List<IntPtr> RemoveList = new List<IntPtr>();
            Thread.Sleep(5000);

            bool IsOnBattery = false;
            bool IsOnBatterySaver = false;
            while (true)
            {
                Thread.Sleep(500);
                MainWindow.SYSTEM_POWER_STATUS sps = new MainWindow.SYSTEM_POWER_STATUS();
                MainWindow.GetSystemPowerStatus(out sps);
                if ((SystemInformation.PowerStatus.PowerLineStatus != System.Windows.Forms.PowerLineStatus.Offline || !PauseOnBat) && (sps.SystemStatusFlag == 0 || !PauseOnBatSave)) // if not on battery or ignoring battery, and if not on battery saver or if ignoring battery saver
                {
                    for (IntPtr wnd = MainWindow.FindWindowExA(IntPtr.Zero, IntPtr.Zero, null, null); wnd != IntPtr.Zero; wnd = MainWindow.FindWindowExA(IntPtr.Zero, wnd, null, null))
                    {
                        int cloaked = -1;
                        MainWindow.DwmGetWindowAttribute(wnd, Convert.ToInt32(DwmApi.DWMWINDOWATTRIBUTE.DWMWA_CLOAKED), out cloaked, Marshal.SizeOf(typeof(bool)));
                        if (MainWindow.IsZoomed(wnd) && MainWindow.IsWindowVisible(wnd) && cloaked <= 0)
                        {
                            MainWindow.GetWindowRect(wnd, out MainWindow.RECT WindowRect);
                            MainWindow.GetWindowRect(VideoPlayerHandle, out MainWindow.RECT VideoPlayerRect);
                            MainWindow.POINT Corner = new MainWindow.POINT
                            {
                                X = VideoPlayerRect.Left + 10,
                                Y = VideoPlayerRect.Top + 10
                            };
                            if (MainWindow.PtInRect(in WindowRect, Corner) && PauseOnMax)
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
                                        ((MainWindow)System.Windows.Application.Current.MainWindow).LogEntry("[ADPVideoPlayer" + LogID + "] Video playing (unobscured)");
                                    });
                                    RemoveList.Add(wnd);

                                }
                            }
                            if (IsOnBattery)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    VideoPlayer.Play();
                                    IsPlaying = true;
                                    ((MainWindow)System.Windows.Application.Current.MainWindow).LogEntry("[ADPVideoPlayer" + LogID + "] Video playing (not on battery)");
                                });
                                IsOnBattery = false;
                            }
                            if (IsOnBatterySaver)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    VideoPlayer.Play();
                                    IsPlaying = true;
                                    ((MainWindow)System.Windows.Application.Current.MainWindow).LogEntry("[ADPVideoPlayer" + LogID + "] Video playing (battery saver disabled)");
                                });
                                IsOnBatterySaver = false;
                            }
                            foreach (IntPtr rwnd in RemoveList)
                            {
                                ObscuringList.Remove(rwnd);
                            }
                            RemoveList.Clear();
                        }
                    }
                }
                else if (SystemInformation.PowerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Offline && PauseOnBat)
                {
                    IsOnBattery = true;
                    Dispatcher.Invoke(() =>
                    {
                        VideoPlayer.Pause();
                        IsPlaying = false;
                        ((MainWindow)System.Windows.Application.Current.MainWindow).LogEntry("[ADPVideoPlayer" + LogID + "] Video paused (on battery)");
                    });
                }
                else if (sps.SystemStatusFlag == 1 && PauseOnBatSave)
                {
                    IsOnBatterySaver = true;
                    Dispatcher.Invoke(() =>
                    {
                        VideoPlayer.Pause();
                        IsPlaying = false;
                        ((MainWindow)System.Windows.Application.Current.MainWindow).LogEntry("[ADPVideoPlayer" + LogID + "] Video paused (battery saver enabled)");
                    });
                }
            }
        }
    }
}

                        
