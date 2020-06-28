using IWshRuntimeLibrary;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using ModernWpf;

namespace ActiveDesktop.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml+		SetRep	{ActiveDesktop.Views.Settings.SettingsRepresentative}	ActiveDesktop.Views.Settings.SettingsRepresentative

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
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                ThemeToggle.IsOn = false;
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
            }
            else
            {
                SetRep.UseDarkTheme = false;
                WriteSettingsJSON();
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
        }

        public class SettingsRepresentative
        {
            public bool UseDarkTheme { get; set; }
        }
    }
}
