using System;
using System.Windows;

namespace ActiveDesktop
{
    /// <summary>
    /// Interaction logic for ADPVideoPlayer.xaml
    /// </summary>
    public partial class ADPVideoWallpaper : Window
    {
        public ADPVideoWallpaper(string path)
        {
            InitializeComponent();
            VideoPlayer.Source = new Uri(path);
            VideoPlayer.Play();
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Position = new TimeSpan(0, 0, 0, 0, 1);
            VideoPlayer.Play();
        }
    }
}
