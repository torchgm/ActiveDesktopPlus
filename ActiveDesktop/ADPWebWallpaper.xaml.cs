using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace ActiveDesktop
{
    /// <summary>
    /// Interaction logic for ADPWebWallpaper.xaml
    /// </summary>
    public partial class ADPWebWallpaper : Window
    {
        string navto = "";
        public ADPWebWallpaper(string url)
        {
            navto = url;
            InitializeComponent();
            WebView.Loaded += WebView_Loaded;
            System.Diagnostics.Debug.WriteLine("Window created");
            Show();
        }

        private void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            Potato();
        }

        private void Potato()
        {
            System.Diagnostics.Debug.WriteLine("Potatoing to " + navto);
            WebView.Navigate(navto);
            System.Diagnostics.Debug.WriteLine("Potatoed");

        }

        private void WebView_NavigationCompleted(object sender, WebViewControlNavigationCompletedEventArgs e)
        {
            if (navto.Contains("youtube.com/watch?v=") || navto.Contains("youtu.be/"))
            {
                System.Diagnostics.Debug.WriteLine("Detected need to trigger fullscreen on YouTube");
                YouTubeFullscreen();
            }
            if (navto.Contains("shadertoy.com/view/"))
            {
                System.Diagnostics.Debug.WriteLine("Detected need to trigger fullscreen on ShaderToy");
                ShaderToyFullscreen();
            }
        }

        private async void YouTubeFullscreen()
        {
            await WebView.InvokeScriptAsync("eval", new string[] { @"document.getElementsByClassName('ytp-fullscreen-button')[0].click();" });
            
            await WebView.InvokeScriptAsync("eval", new string[] { @"document.getElementsByClassName('html5-main-video')[0].loop = true;" });
            
            System.Diagnostics.Debug.WriteLine("Tried and probably failed to make YouTube fullscreen and looping");
        }

        private async void ShaderToyFullscreen()
        {
            await WebView.InvokeScriptAsync("eval", new string[] { "myFullScreen.click();" });
            //await WebView.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName('uiButton')[5].click();" });
            System.Diagnostics.Debug.WriteLine("Tried and probably failed to make ShaderToy fullscreen");
        }
    }
}
