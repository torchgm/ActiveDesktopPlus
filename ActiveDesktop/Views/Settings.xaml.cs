using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using ModernWpf;
using System.Drawing;
using System.Windows.Media;

namespace ActiveDesktop.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : System.Windows.Controls.Page
    {
        public SettingsRepresentative SetRep = new SettingsRepresentative();
        string LocalFolder;

        public Settings()
        {
            InitializeComponent();
            string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            LocalFolder = Path.Combine(AppData, "ActiveDesktopPlus");
            Directory.CreateDirectory(LocalFolder);
            if (!System.IO.File.Exists(Path.Combine(LocalFolder, "settings.json")))
            {
                WriteSettingsJSON();
            }
            if (System.IO.File.ReadAllText(Path.Combine(LocalFolder, "settings.json")) == "" || System.IO.File.ReadAllText(Path.Combine(LocalFolder, "settings.json")) == null)
            {
                SetRep.UseDarkTheme = false;
                WriteSettingsJSON();
            }
            SetRep = ReadSettingsJSON();

            if (SetRep.UseDarkTheme == true)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                ThemeToggle.IsOn = true;
                ((MainWindow)Application.Current.MainWindow).SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                ThemeToggle.IsOn = false;
                ((MainWindow)Application.Current.MainWindow).SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Red);
            }
            if (SetRep.UseDarkTrayIcon == true)
            {
                ((MainWindow)Application.Current.MainWindow).tbi.Icon = ((MainWindow)Application.Current.MainWindow).DarkIcon.Icon;
                TrayIconToggle.IsOn = true;
            }
        }

        public void WriteSettingsJSON()
        {
            System.IO.File.Create(Path.Combine(LocalFolder, "settings.json")).Close();
            System.IO.File.WriteAllText(Path.Combine(LocalFolder, "settings.json"), JsonConvert.SerializeObject(SetRep, Formatting.Indented));
        }

        public SettingsRepresentative ReadSettingsJSON()
        {
            string JsonSettingsList = System.IO.File.ReadAllText(Path.Combine(LocalFolder, "settings.json"));
            SettingsRepresentative sr = JsonConvert.DeserializeObject<SettingsRepresentative>(JsonSettingsList);
            return sr;
        }

        private void StartupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (StartupToggle.IsOn)
            {
                ((MainWindow)Application.Current.MainWindow).EnableStartup(null, null);
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).DisableStartup(null, null);
            }
        }

        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ThemeToggle.IsOn)
            {
                SetRep.UseDarkTheme = true;
                WriteSettingsJSON();
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                ((MainWindow)Application.Current.MainWindow).SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                SetRep.UseDarkTheme = false;
                WriteSettingsJSON();
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                ((MainWindow)Application.Current.MainWindow).SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void TrayIconToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (TrayIconToggle.IsOn)
            {
                SetRep.UseDarkTrayIcon = true;
                ((MainWindow)Application.Current.MainWindow).tbi.Icon = ((MainWindow)Application.Current.MainWindow).DarkIcon.Icon;
                WriteSettingsJSON();
            }
            else
            {
                SetRep.UseDarkTrayIcon = false;
                ((MainWindow)Application.Current.MainWindow).tbi.Icon = ((MainWindow)Application.Current.MainWindow).LightIcon.Icon;
                WriteSettingsJSON();
            }
        }

        public class SettingsRepresentative
        {
            public bool UseDarkTheme { get; set; }
            public bool UseDarkTrayIcon { get; set; }
        }
    }
}
