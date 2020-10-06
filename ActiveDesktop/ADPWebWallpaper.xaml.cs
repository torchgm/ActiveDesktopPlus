using System.Windows;

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
            WebView2.EnsureCoreWebView2Async();
            Show();
            System.Diagnostics.Debug.WriteLine("Window created");
        }

        private void Potato()
        {
            if (navto.Contains("https://www.youtube.com/watch?v="))
            {
                navto = "https://www.youtube.com/embed/" + navto.Substring(32);
            }
            if (navto.Contains("https://youtube.com/watch?v="))
            {
                navto = "https://www.youtube.com/embed/" + navto.Substring(28);
            }
            if (navto.Contains("http://www.youtube.com/watch?v="))
            {
                navto = "https://www.youtube.com/embed/" + navto.Substring(31);
            }
            if (navto.Contains("http://youtube.com/watch?v="))
            {
                navto = "https://www.youtube.com/embed/" + navto.Substring(27);
            }
            if (navto.Contains("https://www.youtu.be/"))
            {
                navto = "https://www.youtube.com/embed/" + navto.Substring(21);
            }
            if (navto.Contains("https://youtu.be/"))
            {
                navto = "https://www.youtube.com/embed/" + navto.Substring(17);
            }
            if (navto.Contains("http://www.youtu.be/"))
            {
                navto = "https://www.youtube.com/embed/" + navto.Substring(20);
            }
            if (navto.Contains("http://youtu.be/"))
            {
                navto = "https://www.youtube.com/embed/" + navto.Substring(16);
            }

            System.Diagnostics.Debug.WriteLine("Potatoing to " + navto);
            WebView2.CoreWebView2.Navigate(navto);
            System.Diagnostics.Debug.WriteLine("Potatoed");
        }

        private void WebView2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            {
                
                if (navto.Contains("shadertoy.com/view/"))
                {
                    System.Diagnostics.Debug.WriteLine("Detected need to trigger fullscreen on ShaderToy");
                    ShaderToyFullscreen();
                }
                if (navto.Contains("youtube.com/embed/"))
                {
                    YouTubeFullscreen();
                }
            }
        }

        private async void YouTubeFullscreen()
        {
            await WebView2.ExecuteScriptAsync("document.getElementsByClassName('ytp-large-play-button')[0].click();");

            await WebView2.ExecuteScriptAsync("document.getElementsByClassName('ytp-fullscreen-button')[0].click();");
            
            await WebView2.ExecuteScriptAsync("document.getElementsByClassName('ytp-mute-button')[0].click();");

            await WebView2.ExecuteScriptAsync("document.getElementsByClassName('html5-main-video')[0].loop = true;");


            System.Diagnostics.Debug.WriteLine("Tried and probably failed to make YouTube fullscreen and looping");
        }

        private async void ShaderToyFullscreen()
        {
            await WebView2.ExecuteScriptAsync("myFullScreen.click();");
            System.Diagnostics.Debug.WriteLine("Tried and probably failed to make ShaderToy fullscreen");
        }

        private void WebView2_CoreWebView2Ready(object sender, System.EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Potato();
        }
    }
}
