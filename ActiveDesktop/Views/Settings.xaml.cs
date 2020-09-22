using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using ModernWpf;
using DesktopBridge;
using System.Windows.Media;
using System.Diagnostics;
using Microsoft.Win32;

namespace ActiveDesktop.Views
{
    public partial class Settings : System.Windows.Controls.Page
    {
        public SettingsRepresentative SetRep = new SettingsRepresentative();
        string LocalFolder;
        MainWindow mw;

        public Settings()
        {
            InitializeComponent();
            mw = (MainWindow)Application.Current.MainWindow;

            int AppColour = Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\DWM", "AccentColor", null));
            ThemeManager.Current.AccentColor = Color.FromRgb((byte)AppColour, (byte)(AppColour >> 8), (byte)(AppColour >> 16));
            

            string TargetFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (mw.IsRunningAsUWP())
            {
                LocalFolder = Windows.Storage.ApplicationData.Current.RoamingFolder.Path;
            }
            else
            {
                LocalFolder = Path.Combine(TargetFolder, "ActiveDesktopPlus");
                Directory.CreateDirectory(LocalFolder);
            }


            if (!File.Exists(Path.Combine(LocalFolder, "settings.json")))
            {
                WriteSettingsJSON();
            }
            if (File.ReadAllText(Path.Combine(LocalFolder, "settings.json")) == "" || File.ReadAllText(Path.Combine(LocalFolder, "settings.json")) == null)
            {
                SetRep.UseDarkTheme = false;
                SetRep.PauseOnBattery = true;
                SetRep.UseSystemTheme = true;
                SetRep.PauseOnMaximise = true;
                SetRep.DebugMode = false;
                WriteSettingsJSON();
            }
            SetRep = ReadSettingsJSON();

            if (SetRep.UseDarkTheme)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                ThemeToggle.IsOn = true;
                mw.SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Yellow);
                StartupWarningLabel.Foreground = new SolidColorBrush(Colors.Yellow);
                mw.ErrorIcon.Foreground = new SolidColorBrush(Colors.Yellow);
                mw.ImmersiveFinalisePage.WarningBlock.Foreground = new SolidColorBrush(Colors.Yellow);
                mw.ImmersiveExperiencePage.AddWallpaperIcon.Foreground = new SolidColorBrush(Colors.White);
                mw.ImmersiveFinalisePage.CheckmarkIcon.Foreground = new SolidColorBrush(Colors.White);
                mw.ImmersiveExperiencePage.CurrentColour = new SolidColorBrush(Colors.White);
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                ThemeToggle.IsOn = false;
                mw.SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Red);
                StartupWarningLabel.Foreground = new SolidColorBrush(Colors.Red);
                mw.ErrorIcon.Foreground = new SolidColorBrush(Colors.Red);
                mw.ImmersiveFinalisePage.WarningBlock.Foreground = new SolidColorBrush(Colors.Red);
                mw.ImmersiveExperiencePage.AddWallpaperIcon.Foreground = new SolidColorBrush(Colors.Black);
                mw.ImmersiveFinalisePage.CheckmarkIcon.Foreground = new SolidColorBrush(Colors.Black);
                mw.ImmersiveExperiencePage.CurrentColour = new SolidColorBrush(Colors.Black);
            }
            if (SetRep.UseDarkTrayIcon)
            {
                mw.tbi.Icon = mw.DarkIcon.Icon;
                TrayIconToggle.IsOn = true;
            }
            if (SetRep.UseSystemTheme)
            {
                SystemThemeCheckBox_Checked(null, null);
                SystemThemeCheckBox.IsChecked = true;
            }
            else
            {
                SystemThemeCheckBox_Unchecked(null, null);
            }
            if (SetRep.PauseOnMaximise)
            {
                PauseMaximisedToggle.IsOn = true;
                mw.PauseOnMaximise = true;
            }
            else
            {
                mw.PauseOnMaximise = false;
            }
            if (SetRep.PauseOnBatterySaver)
            {
                PauseBatterySaverToggle.IsOn = true;
                mw.PauseOnBatterySaver = true;
            }
            else
            {
                mw.PauseOnBatterySaver = false;
            }
            if (SetRep.PauseOnBattery)
            {
                PauseBatteryToggle.IsOn = true;
                mw.PauseOnBattery = true;
                mw.PauseOnBatterySaver = true;
                PauseBatterySaverToggle.IsOn = true;
                PauseBatterySaverToggle.IsEnabled = false;
            }
            else
            {
                mw.PauseOnBattery = false;
            }
            if (SetRep.DebugMode)
            {
                DebugModeToggle.IsOn = true;
                mw.DebugMode = true;
            }
            else
            {
                mw.DebugMode = false;
            }

        }

        public void WriteSettingsJSON()
        {
            File.Create(Path.Combine(LocalFolder, "settings.json")).Close();
            File.WriteAllText(Path.Combine(LocalFolder, "settings.json"), JsonConvert.SerializeObject(SetRep, Formatting.Indented));
        }

        public SettingsRepresentative ReadSettingsJSON()
        {
            string JsonSettingsList = File.ReadAllText(Path.Combine(LocalFolder, "settings.json"));
            SettingsRepresentative sr = JsonConvert.DeserializeObject<SettingsRepresentative>(JsonSettingsList);
            return sr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void StartupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (mw.IsRunningAsUWP())
            {

            }
            else
            {
                if (StartupToggle.IsOn)
                {
                    mw.EnableStartup(null, null);
                    mw.LogEntry("[SET] Startup enabled");
                }
                else
                {
                    mw.DisableStartup(null, null);
                    mw.LogEntry("[SET] Startup disabled");
                }
            }
            
        }

        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ThemeToggle.IsOn)
            {
                SetRep.UseDarkTheme = true;
                WriteSettingsJSON();
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                mw.SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Yellow);
                StartupWarningLabel.Foreground = new SolidColorBrush(Colors.Yellow);
                mw.ErrorIcon.Foreground = new SolidColorBrush(Colors.Yellow);
                mw.ImmersiveFinalisePage.WarningBlock.Foreground = new SolidColorBrush(Colors.Yellow);
                mw.ImmersiveExperiencePage.AddWallpaperIcon.Foreground = new SolidColorBrush(Colors.White);
                mw.ImmersiveFinalisePage.CheckmarkIcon.Foreground = new SolidColorBrush(Colors.White);
                mw.ImmersiveExperiencePage.CurrentColour = new SolidColorBrush(Colors.White);
                mw.LogEntry("[SET] Switched to dark theme");
            }
            else
            {
                SetRep.UseDarkTheme = false;
                WriteSettingsJSON();
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                mw.SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Red);
                StartupWarningLabel.Foreground = new SolidColorBrush(Colors.Red);
                mw.ErrorIcon.Foreground = new SolidColorBrush(Colors.Red);
                mw.ImmersiveFinalisePage.WarningBlock.Foreground = new SolidColorBrush(Colors.Red);
                mw.ImmersiveExperiencePage.AddWallpaperIcon.Foreground = new SolidColorBrush(Colors.Black);
                mw.ImmersiveFinalisePage.CheckmarkIcon.Foreground = new SolidColorBrush(Colors.Black);
                mw.ImmersiveExperiencePage.CurrentColour = new SolidColorBrush(Colors.Black);
                mw.LogEntry("[SET] Switched to light theme");

            }
        }

        private void TrayIconToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (TrayIconToggle.IsOn)
            {
                SetRep.UseDarkTrayIcon = true;
                mw.tbi.Icon = mw.DarkIcon.Icon;
                mw.LogEntry("[SET] Switched to dark tray icon");

                WriteSettingsJSON();
            }
            else
            {
                SetRep.UseDarkTrayIcon = false;
                mw.tbi.Icon = mw.LightIcon.Icon;
                mw.LogEntry("[SET] Switched to light tray icon");

                WriteSettingsJSON();
            }
        }

        private void SystemThemeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SetRep.UseSystemTheme = true;
            ActOnSystemTheme();
            TrayIconToggle.IsEnabled = false;
            ThemeToggle.IsEnabled = false;
            WriteSettingsJSON();
        }

        private void SystemThemeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetRep.UseSystemTheme = false;
            TrayIconToggle_Toggled(null, null);
            ThemeToggle_Toggled(null, null);
            TrayIconToggle.IsEnabled = true;
            ThemeToggle.IsEnabled = true;
            WriteSettingsJSON();
        }

        // Returns true if light, false if dark
        private void ActOnSystemTheme()
        {
            int AppsUseLightTheme = Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", null));
            int SystemUsesLightTheme = Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", null));

            if (AppsUseLightTheme == 1)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                mw.SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Red);
                StartupWarningLabel.Foreground = new SolidColorBrush(Colors.Red);
                mw.ErrorIcon.Foreground = new SolidColorBrush(Colors.Red);
                mw.ImmersiveFinalisePage.WarningBlock.Foreground = new SolidColorBrush(Colors.Red);
                mw.ImmersiveExperiencePage.AddWallpaperIcon.Foreground = new SolidColorBrush(Colors.Black);
                mw.ImmersiveFinalisePage.CheckmarkIcon.Foreground = new SolidColorBrush(Colors.Black);
                mw.ImmersiveExperiencePage.CurrentColour = new SolidColorBrush(Colors.Black);
                mw.LogEntry("[SET] Switched to light theme (following system)");
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                mw.SavedAppsPage.WriteButton.Foreground = new SolidColorBrush(Colors.Yellow);
                StartupWarningLabel.Foreground = new SolidColorBrush(Colors.Yellow);
                mw.ErrorIcon.Foreground = new SolidColorBrush(Colors.Yellow);
                mw.ImmersiveFinalisePage.WarningBlock.Foreground = new SolidColorBrush(Colors.Yellow);
                mw.ImmersiveExperiencePage.AddWallpaperIcon.Foreground = new SolidColorBrush(Colors.White);
                mw.ImmersiveFinalisePage.CheckmarkIcon.Foreground = new SolidColorBrush(Colors.White);
                mw.ImmersiveExperiencePage.CurrentColour = new SolidColorBrush(Colors.White);
                mw.LogEntry("[SET] Switched to dark theme (following system)");
            }

            if (SystemUsesLightTheme == 1)
            {
                mw.tbi.Icon = mw.DarkIcon.Icon;
                mw.LogEntry("[SET] Switched to dark tray icon (following system)");
            }
            else
            {
                mw.tbi.Icon = mw.LightIcon.Icon;
                mw.LogEntry("[SET] Switched to light tray icon (following system)");
            }
        }

        private void PauseBatteryToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (PauseBatteryToggle.IsOn)
            {
                SetRep.PauseOnBattery = true;
                mw.PauseOnBattery = true;
                SetRep.PauseOnBatterySaver = true;
                mw.PauseOnBatterySaver = true;
                PauseBatterySaverToggle.IsOn = true;
                PauseBatterySaverToggle.IsEnabled = false;
                mw.LogEntry("[SET] Enabled pausing on battery");

                WriteSettingsJSON();
            }
            else
            {
                SetRep.PauseOnBattery = false;
                mw.PauseOnBattery = false;
                PauseBatterySaverToggle.IsEnabled = true;
                mw.LogEntry("[SET] Disabled pausing on battery");

                WriteSettingsJSON();
            }
        }

        private void PauseMaximisedToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (PauseMaximisedToggle.IsOn)
            {
                SetRep.PauseOnMaximise = true;
                mw.PauseOnMaximise = true;
                mw.LogEntry("[SET] Enabled pausing on maximised");

                WriteSettingsJSON();
            }
            else
            {
                SetRep.PauseOnMaximise = false;
                mw.PauseOnMaximise = false;
                mw.LogEntry("[SET] Disabled pausing on maximised");

                WriteSettingsJSON();
            }
        }

        private void PauseBatterySaverToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (PauseBatterySaverToggle.IsOn)
            {
                SetRep.PauseOnBatterySaver = true;
                mw.PauseOnBatterySaver = true;
                mw.LogEntry("[SET] Enabled pausing on Battery Saver");

                WriteSettingsJSON();
            }
            else
            {
                SetRep.PauseOnBatterySaver = false;
                mw.PauseOnBatterySaver = false;
                mw.LogEntry("[SET] Disabled Pausing on Battery Saver");
                
                WriteSettingsJSON();
            }
        }

        private void DebugModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (DebugModeToggle.IsOn)
            {
                SetRep.DebugMode = true;
                mw.DebugMode = true;
                mw.DebugPageForToggling.Visibility = Visibility.Visible;
                mw.LogEntry("[SET] Enabled debug mode");

                WriteSettingsJSON();
            }
            else
            {
                mw.LogEntry("[SET] Disabled debug mode");
                SetRep.DebugMode = false;
                mw.DebugMode = false;
                mw.DebugPageForToggling.Visibility = Visibility.Hidden;

                WriteSettingsJSON();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public class SettingsRepresentative
        {
            public bool UseDarkTheme { get; set; }
            public bool UseDarkTrayIcon { get; set; }
            public bool UseSystemTheme { get; set; }
            public bool PauseOnMaximise { get; set; }
            public bool PauseOnBattery { get; set; }
            public bool PauseOnBatterySaver { get; set; }
            public bool DebugMode { get; set; }
        }

        private void EnableInSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("ms-settings:startupapps");
        }


    }
}
