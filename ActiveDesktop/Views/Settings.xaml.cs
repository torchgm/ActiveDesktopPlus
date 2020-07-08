using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using ModernWpf;
using System.Drawing;
using System.Windows.Media;

namespace ActiveDesktop.Views
{
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
            if (!File.Exists(Path.Combine(LocalFolder, "settings.json")))
            {
                WriteSettingsJSON();
            }
            if (File.ReadAllText(Path.Combine(LocalFolder, "settings.json")) == "" || System.IO.File.ReadAllText(Path.Combine(LocalFolder, "settings.json")) == null)
            {
                SetRep.UseDarkTheme = false;
                SetRep.PauseOnBattery = true;
                SetRep.PauseOnMaximise = true;
                SetRep.DebugMode = false;
                WriteSettingsJSON();
            }
            SetRep = ReadSettingsJSON();

            if (SetRep.UseDarkTheme)
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
            if (SetRep.UseDarkTrayIcon)
            {
                ((MainWindow)Application.Current.MainWindow).tbi.Icon = ((MainWindow)Application.Current.MainWindow).DarkIcon.Icon;
                TrayIconToggle.IsOn = true;
            }
            if (SetRep.PauseOnMaximise)
            {
                PauseMaximisedToggle.IsOn = true;
                ((MainWindow)Application.Current.MainWindow).PauseOnMaximise = true;
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).PauseOnMaximise = false;
            }
            if (SetRep.PauseOnBatterySaver)
            {
                PauseBatterySaverToggle.IsOn = true;
                ((MainWindow)Application.Current.MainWindow).PauseOnBatterySaver = true;
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).PauseOnBatterySaver = false;
            }
            if (SetRep.PauseOnBattery)
            {
                PauseBatteryToggle.IsOn = true;
                ((MainWindow)Application.Current.MainWindow).PauseOnBattery = true;
                ((MainWindow)Application.Current.MainWindow).PauseOnBatterySaver = true;
                PauseBatterySaverToggle.IsOn = true;
                PauseBatterySaverToggle.IsEnabled = false;
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).PauseOnBattery = false;
            }
            if (SetRep.DebugMode)
            {
                DebugModeToggle.IsOn = true;
                ((MainWindow)Application.Current.MainWindow).DebugMode = true;
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).DebugMode = false;
            }
        }

        public void WriteSettingsJSON()
        {
            File.Create(Path.Combine(LocalFolder, "settings.json")).Close();
            File.WriteAllText(Path.Combine(LocalFolder, "settings.json"), JsonConvert.SerializeObject(SetRep, Formatting.Indented));
        }

        public SettingsRepresentative ReadSettingsJSON()
        {
            string JsonSettingsList = System.IO.File.ReadAllText(Path.Combine(LocalFolder, "settings.json"));
            SettingsRepresentative sr = JsonConvert.DeserializeObject<SettingsRepresentative>(JsonSettingsList);
            return sr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void StartupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (StartupToggle.IsOn)
            {
                ((MainWindow)Application.Current.MainWindow).EnableStartup(null, null);
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Startup enabled");
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).DisableStartup(null, null);
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Startup disabled");
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
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Switched to dark theme");
            }
            else
            {
                SetRep.UseDarkTheme = false;
                WriteSettingsJSON();
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                ((MainWindow)Application.Current.MainWindow).SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Red);
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Switched to light theme");
            }
        }

        private void TrayIconToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (TrayIconToggle.IsOn)
            {
                SetRep.UseDarkTrayIcon = true;
                ((MainWindow)Application.Current.MainWindow).tbi.Icon = ((MainWindow)Application.Current.MainWindow).DarkIcon.Icon;
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Switched to dark tray icon");

                WriteSettingsJSON();
            }
            else
            {
                SetRep.UseDarkTrayIcon = false;
                ((MainWindow)Application.Current.MainWindow).tbi.Icon = ((MainWindow)Application.Current.MainWindow).LightIcon.Icon;
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Switched to light tray icon");

                WriteSettingsJSON();
            }
        }

        private void PauseBatteryToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (PauseBatteryToggle.IsOn)
            {
                SetRep.PauseOnBattery = true;
                ((MainWindow)Application.Current.MainWindow).PauseOnBattery = true;
                SetRep.PauseOnBatterySaver = true;
                ((MainWindow)Application.Current.MainWindow).PauseOnBatterySaver = true;
                PauseBatterySaverToggle.IsOn = true;
                PauseBatterySaverToggle.IsEnabled = false;
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Enabled pausing on battery");

                WriteSettingsJSON();
            }
            else
            {
                SetRep.PauseOnBattery = false;
                ((MainWindow)Application.Current.MainWindow).PauseOnBattery = false;
                PauseBatterySaverToggle.IsEnabled = true;
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Disabled pausing on battery");

                WriteSettingsJSON();
            }
        }

        private void PauseMaximisedToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (PauseMaximisedToggle.IsOn)
            {
                SetRep.PauseOnMaximise = true;
                ((MainWindow)Application.Current.MainWindow).PauseOnMaximise = true;
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Enabled pausing on maximised");

                WriteSettingsJSON();
            }
            else
            {
                SetRep.PauseOnMaximise = false;
                ((MainWindow)Application.Current.MainWindow).PauseOnMaximise = false;
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Disabled pausing on maximised");

                WriteSettingsJSON();
            }
        }

        private void PauseBatterySaverToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (PauseBatterySaverToggle.IsOn)
            {
                SetRep.PauseOnBatterySaver = true;
                ((MainWindow)Application.Current.MainWindow).PauseOnBatterySaver = true;
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Enabled pausing on Battery Saver");

                WriteSettingsJSON();
            }
            else
            {
                SetRep.PauseOnBatterySaver = false;
                ((MainWindow)Application.Current.MainWindow).PauseOnBatterySaver = false;
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Disabled Pausing on Battery Saver");
                
                WriteSettingsJSON();
            }
        }

        private void DebugModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (DebugModeToggle.IsOn)
            {
                SetRep.DebugMode = true;
                ((MainWindow)Application.Current.MainWindow).DebugMode = true;
                ((MainWindow)Application.Current.MainWindow).DebugPageForToggling.Visibility = Visibility.Visible;
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Enabled debug mode");

                WriteSettingsJSON();
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).LogEntry("[Settings] Disabled debug mode");
                SetRep.DebugMode = false;
                ((MainWindow)Application.Current.MainWindow).DebugMode = false;
                ((MainWindow)Application.Current.MainWindow).DebugPageForToggling.Visibility = Visibility.Hidden;

                WriteSettingsJSON();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public class SettingsRepresentative
        {
            public bool UseDarkTheme { get; set; }
            public bool UseDarkTrayIcon { get; set; }
            public bool PauseOnMaximise { get; set; }
            public bool PauseOnBattery { get; set; }
            public bool PauseOnBatterySaver { get; set; }
            public bool DebugMode { get; set; }
        }


    }
}
