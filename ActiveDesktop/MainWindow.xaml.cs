using ActiveDesktop.Views;
using IWshRuntimeLibrary;
using ModernWpf.Controls;
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;


namespace ActiveDesktop
{
    public partial class MainWindow : Window
    {
        // I'm sure it's bad practice to ever declare anything up here ever but screw that I'm doing it anyway
        // It's easier and I need them everywhere so shh it'll be fine I promise aaaa
        IntPtr DesktopHandle; // The handle of the desktop
        IntPtr TargetHandle; // The handle of the targeted app
        string LocalFolder; // %AppData%/ActiveDesktopPlus
        List<App> JSONArrayList = new List<App>(); // ArrayList that holds data from JSON for on-the-fly reading and writing or something
        List<List<string>> DesktopWindowPropertyList; // Not entirely sure, probably something to do with the children of the desktop
        List<int> WindowHandles = new List<int>(); // List of handles
        List<int> WindowProperties = new List<int>(); // List of window properties
        List<int> WindowPropertiesEx = new List<int>(); // List of window properties but this time its ex
        public DisplayInfoCollection Displays = new DisplayInfoCollection(); // List of displays and their properties
        int SelectedDisplay = -1; // Selected Display that needs to probably be global or smth
        public bool IsHidden = false; // Keeps track of whether or not the window is hidden
        
        //
        public bool PauseOnBattery = true;
        public bool PauseOnMaximise = true;
        public bool PauseOnBatterySaver = true;

        // Generates pages
        public Settings SettingsPage;
        public CurrentApps CurrentAppsPage;
        public SavedApps SavedAppsPage;
        public Help HelpPage;

        // On-start tasks
        public MainWindow()
        {
            // Important thingies
            InitializeComponent();
            CurrentAppsPage = new CurrentApps();
            SavedAppsPage = new SavedApps();
            SettingsPage = new Settings();
            HelpPage = new Help();
            tbi.Visibility = Visibility.Visible;
            
            // Find and assign desktop handle because microsoft dumb and this can't just be the same thing each boot
            IntPtr RootHandle = FindWindowExA(IntPtr.Zero, IntPtr.Zero, "Progman", "Program Manager");
            DesktopHandle = FindWindowExA(RootHandle, IntPtr.Zero, "SHELLDLL_DefView", "");
            while (DesktopHandle == IntPtr.Zero)
            {
                RootHandle = FindWindowExA(IntPtr.Zero, RootHandle, "WorkerW", "");
                DesktopHandle = FindWindowExA(RootHandle, IntPtr.Zero, "SHELLDLL_DefView", "");
            }

            // Trigger a refresh of many things. Not strictly necessary for all of this but hey extra refreshing is always nice
            FileSystem();
            JSONArrayList = ReadJSON();
            
            Displays = GetDisplays();
            RefreshLists();
            if (JSONArrayList.Count != 0 && WindowHandles.Count() == 0)
            {
                StartSavedApps();
            }
            if (!System.IO.File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Active Desktop Plus.lnk")))
            {
                SettingsPage.StartupToggle.IsOn = false;
            }
            else
            {
                SettingsPage.StartupToggle.IsOn = true;
            }

            CurrentAppsPage.TitleTextBox.Text = "[Hold Ctrl to select an app]";
            CurrentAppsPage.HwndInputTextBox.Text = "";

        }

        // Stores window properties
        public void StoreWindowProperties(int Handle, int Properties, int PropertiesEx)
        {
            bool appears = false;
            for (int i = 0; i < WindowHandles.Count; i++)
            {
                if (WindowHandles[i] == Handle)
                {
                    appears = true;
                }
            }
            if (appears == false)
            {
                WindowHandles.Add(Handle);
                WindowProperties.Add(Properties);
                WindowPropertiesEx.Add(PropertiesEx);

            }
        }

        // Retrieves window properties
        public int RetrieveWindowProperties(int Handle, int Ex)
        {
            for (int i = 0; i < WindowHandles.Count; i++)
            {
                if (WindowHandles[i] == Handle && Ex == 1)
                {
                    return Convert.ToInt32(WindowPropertiesEx[i]);
                }
                else if (WindowHandles[i] == Handle)
                {
                    return Convert.ToInt32(WindowProperties[i]);
                }
            }
            return 0;
        }

        // Deals with listening to the Ctrl key
        public void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            // Deals with finding the target window the user wants to send to the desktop
            // I'll probably work out how to do it with the mouse at some point but 🅱 was easier at the time
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                TargetHandle = WindowFromPoint(Convert.ToInt32(GetCursorPosition().X), Convert.ToInt32(GetCursorPosition().Y));
                CurrentAppsPage.HwndInputTextBox.Text = TargetHandle.ToString();
                StringBuilder WindowTitle = new StringBuilder(1000);
                int result = GetWindowText(TargetHandle, WindowTitle, 1000);
                CurrentAppsPage.TitleTextBox.Text = WindowTitle.ToString();
                if (CurrentAppsPage.HwndInputTextBox.Text == DesktopHandle.ToString())
                {
                    CurrentAppsPage.TitleTextBox.Text = "[Desktop]";
                }
                else if (CurrentAppsPage.TitleTextBox.Text == "")
                {
                    CurrentAppsPage.TitleTextBox.Text = "[Window has no title]";
                }
                CurrentAppsPage.ApplyHwndButton.Content = "Hover over an app's title bar\nto select it, then release Ctrl";
                CurrentAppsPage.ApplyHwndButton.IsEnabled = false;
            }
        }

        // See above
        public void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            CurrentAppsPage.ApplyHwndButton.Content = "Send to Desktop";
            CurrentAppsPage.ApplyHwndButton.IsEnabled = true;
        }

        // Sends the selected handle to the desktop
        public void ApplyHwndButton_Click(object sender, RoutedEventArgs e)
        {
            // This bit just makes the window a child of the desktop. Honestly a lot easier than I first thought.
            RECT TempRect = new RECT();
            GetWindowRect(TargetHandle, out TempRect);
            SetParent(TargetHandle, DesktopHandle);
            int x = TranslateCanvasX(0);
            int y = TranslateCanvasY(0);
            MoveWindow(TargetHandle, (TempRect.Left + x), (TempRect.Top + y), Convert.ToInt32(GetWindowSize(TargetHandle).Width), Convert.ToInt32(GetWindowSize(TargetHandle).Height), true);
            CurrentAppsPage.TitleTextBox.Text = "[Hold Ctrl to select an app]";
            CurrentAppsPage.HwndInputTextBox.Text = "";

            RefreshLists();
        }

        // Magic
        protected override void OnActivated(EventArgs e)
        {
            RefreshLists();
            base.OnActivated(e);
        }

        // Makes the selected window borderless
        public void BorderlessButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAppsPage.HandleListBox.SelectedItem != null)
            {
                PinApp(DesktopWindowPropertyList[Convert.ToInt32(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Substring(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Length - 3))][2]);
            }
        }

        // Refresh event for children of the desktop
        public void RefreshLists()
        {
            List<List<string>> OldList = DesktopWindowPropertyList;

            // Quite frankly I've forgotten how this bit works but it needs to be a StringBuilder and I just hope window titles aren't too long
            StringBuilder WindowTitle = new StringBuilder(1000);
            int count = 0;
            DesktopWindowPropertyList = new List<List<string>>();

            // Checks every child of the desktop to see if they're a direct root window, then if they are, assigns them a numerical ID and adds their title and handle to a list.
            CurrentAppsPage.HandleListBox.Items.Clear();
            for (IntPtr ChildHandle = FindWindowExA(DesktopHandle, IntPtr.Zero, null, null); ChildHandle != IntPtr.Zero; ChildHandle = FindWindowExA(DesktopHandle, ChildHandle, null, null))
            {
                int result = GetWindowText(ChildHandle, WindowTitle, 1000);
                if (result != 1001 && WindowTitle.ToString() != "FolderView")
                {
                    // This bit is the clever bit that creates an entry for every window on the desktop
                    DesktopWindowPropertyList.Add(new List<string>());
                    DesktopWindowPropertyList[count].Add((1000 + count).ToString()); // ID - Nobody is ever going to have more than 1000 windows open, if they do this will break but hey idc
                    DesktopWindowPropertyList[count].Add(WindowTitle.ToString()); // Window Title
                    DesktopWindowPropertyList[count].Add(ChildHandle.ToString()); // Window Handle
                    StoreWindowProperties(ChildHandle.ToInt32(), (int)GetWindowLong(ChildHandle, WeirdMagicalNumbers.GWL_STYLE), (int)GetWindowLong(ChildHandle, WeirdMagicalNumbers.GWL_EXSTYLE));
                    DesktopWindowPropertyList[count].Add("false");
                    if (OldList != null)
                    {
                        foreach (List<string> i in OldList)
                        {
                            if (i[2] == DesktopWindowPropertyList[count][2])
                            {
                                DesktopWindowPropertyList[count][3] = i[3];
                            }
                        }
                    }

                    CurrentAppsPage.HandleListBox.Items.Add(DesktopWindowPropertyList[count][1] + " " + DesktopWindowPropertyList[count][0]);
                    count++;
                }
            }
            SavedListRefreshEvent();
        }

        // Refresh event for the saved apps list
        public void SavedListRefreshEvent()
        {
            SavedAppsPage.SavedListBox.Items.Clear();
            foreach (App i in JSONArrayList)
            {
                if (i.Name == "Friendly Name")
                {
                    SavedAppsPage.SavedListBox.Items.Add(i.Cmd);

                }
                else
                {
                    SavedAppsPage.SavedListBox.Items.Add(i.Name);
                }
            }
        }

        // Handles adding a new app to the saved apps list
        public void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedAppsPage.CmdBox.Text != "Command Line" && !(SavedAppsPage.CmdBox.Text == "MEDIA" && SavedAppsPage.FlagBox.Text == "Path to Video"))
            {
                App AppToAdd = new App
                {
                    Cmd = SavedAppsPage.CmdBox.Text,
                    Xpos = SavedAppsPage.XBox.Text,
                    Ypos = SavedAppsPage.YBox.Text,
                    Width = SavedAppsPage.WidthBox.Text,
                    Height = SavedAppsPage.HeightBox.Text,
                    Flags = SavedAppsPage.FlagBox.Text,
                    Name = SavedAppsPage.NameBox.Text,
                    Time = SavedAppsPage.TimeBox.Text,
                    Lock = SavedAppsPage.LockedCheckBox.IsChecked ?? false,
                    Startup = SavedAppsPage.AutostartCheckBox.IsChecked ?? false,
                    Fix = SavedAppsPage.FixCheckBox.IsChecked ?? false,
                    Pin = SavedAppsPage.PinnedCheckBox.IsChecked ?? false
                };
                JSONArrayList.Add(AppToAdd);



                SavedAppsPage.CmdBox.Text = "Command Line";
                SavedAppsPage.CmdBox.IsEnabled = true;
                SavedAppsPage.XBox.Text = "X";
                SavedAppsPage.XBox.IsEnabled = true;
                SavedAppsPage.YBox.Text = "Y";
                SavedAppsPage.YBox.IsEnabled = true;
                SavedAppsPage.WidthBox.Text = "Width";
                SavedAppsPage.WidthBox.IsEnabled = true;
                SavedAppsPage.HeightBox.Text = "Height";
                SavedAppsPage.HeightBox.IsEnabled = true;
                SavedAppsPage.FlagBox.Text = "Flags";
                SavedAppsPage.NameBox.Text = "Friendly Name";
                SavedAppsPage.TimeBox.Text = "Wait Time";
                SavedAppsPage.WriteButton.IsEnabled = true;
                SavedAppsPage.MediaButton.Content = "  Use \nVideo";

            }
            SavedListRefreshEvent();
        }

        // Calls a refresh of the children of the desktop
        public void AppList_Click(object sender, MouseButtonEventArgs e)
        {
            RefreshLists();
        }

        // Button that removes entries from the saved app list
        public void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedAppsPage.SavedListBox.SelectedIndex != -1)
            {
                JSONArrayList.RemoveAt(SavedAppsPage.SavedListBox.SelectedIndex);
                SavedListRefreshEvent();
                SavedAppsPage.WriteButton.IsEnabled = true;
            }

        }

        // Button that writes changes to disk
        public void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            WriteJSON();
            SavedAppsPage.WriteButton.IsEnabled = false;
        }

        // Button that tests how an application behaves on startup
        public void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedAppsPage.SavedListBox.SelectedIndex != -1)
            {
                int n = 0;
                foreach (App i in JSONArrayList)
                {
                    int t = 1000;
                    if (i.Time != "Wait Time")
                    {
                        try
                        {
                            t = Convert.ToInt32(i.Time);
                        }
                        catch (Exception) { }
                    }

                    if (i.Flags == "Flags")
                    {
                        i.Flags = string.Empty;
                    }

                    if (i.Flags == "Path to Video")
                    {
                        i.Flags = @"C:\.mp4";
                    }

                    if (n == SavedAppsPage.SavedListBox.SelectedIndex)
                    {
                        WindowFromListToDesktop(i, t);
                    }


                    ++n;
                }
            }
            RefreshLists();
        }

        // Draws a locking window over any given app
        public void LockButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAppsPage.HandleListBox.SelectedItem != null)
            {
                IntPtr hwnd = new IntPtr(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Substring(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Length - 3))][2]));
                LockApp(hwnd);
            }
        }

        // Allows the user to easily save the selected window
        public void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAppsPage.HandleListBox.SelectedIndex != -1)
            {
                string SyllyIsAwesome;
                IntPtr hwnd = IntPtr.Zero;
                hwnd = new IntPtr(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Substring(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Length - 3))][2]));
                StringBuilder WindowTitle = new StringBuilder(1000);
                StringBuilder FileName = new StringBuilder(1000);
                uint size = (uint)FileName.Capacity;
                GetWindowThreadProcessId(hwnd, out uint PID);
                IntPtr handle = OpenProcess(0x1000, false, PID);
                if (QueryFullProcessImageNameW(handle, 0, FileName, ref size) != 0)
                {
                    SyllyIsAwesome = FileName.ToString(0, (int)size);
                }
                else
                {
                    SyllyIsAwesome = string.Empty;
                }
                CloseHandle(handle);

                GetWindowRect(hwnd, out RECT PosTarget);
                GetWindowText(hwnd, WindowTitle, 1000);
                SavedAppsPage.CmdBox.Text = SyllyIsAwesome;
                SavedAppsPage.XBox.Text = PosTarget.Left.ToString();
                SavedAppsPage.YBox.Text = PosTarget.Top.ToString();
                SavedAppsPage.WidthBox.Text = GetWindowSize(hwnd).Width.ToString();
                SavedAppsPage.HeightBox.Text = GetWindowSize(hwnd).Height.ToString();
                SavedAppsPage.NameBox.Text = WindowTitle.ToString();
                ContentFrame.Navigate(SavedAppsPage);
                NavView.SelectedItem = NavView.MenuItems[1];
            }
        }

        // Attempts to close the selected app
        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAppsPage.HandleListBox.SelectedItem != null)
            {
                IntPtr hwnd = new IntPtr(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Substring(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Length - 3))][2]));
                uint WM_CLOSE = 0x0010;
                SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            Thread.Sleep(250);
            RefreshLists();
        }

        // Adds startup shortcut
        public void EnableStartup(object sender, RoutedEventArgs e)
        {
            try
            {
                string shortcutFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (!Directory.Exists(shortcutFolder))
                {
                    Directory.CreateDirectory(shortcutFolder);
                }
                WshShell shellClass = new WshShell();
                string ADPStartupLink = Path.Combine(shortcutFolder, "Active Desktop Plus.lnk");
                IWshShortcut shortcut = (IWshShortcut)shellClass.CreateShortcut(ADPStartupLink);
                shortcut.TargetPath = Environment.GetCommandLineArgs()[0];
                shortcut.IconLocation = Environment.GetCommandLineArgs()[0];
                shortcut.Arguments = "";
                shortcut.Description = "Start Active Desktop Plus";
                shortcut.Save();
            }
            catch (Exception)
            {

                throw;
            }
        }

        // Removes startup shortcut
        public void DisableStartup(object sender, RoutedEventArgs e)
        {
            try
            {
                string shortcutFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string ADPStartupLink = Path.Combine(shortcutFolder, "Active Desktop Plus.lnk");
                System.IO.File.Delete(ADPStartupLink);
            }
            catch (Exception)
            {

                throw;
            }
        }

        // Button for selecting a monitor quickly
        public void MonitorSelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedDisplay++;
            if (SelectedDisplay > Displays.Count)
            {
                SelectedDisplay = 0;
            }
            SavedAppsPage.MonitorSelectButton.Content = "Monitor:\n      " + (SelectedDisplay + 1).ToString();
            try
            {
                SavedAppsPage.XBox.Text = Displays[SelectedDisplay].MonitorArea.Left.ToString();
                SavedAppsPage.YBox.Text = Displays[SelectedDisplay].MonitorArea.Top.ToString();
                SavedAppsPage.WidthBox.Text = Displays[SelectedDisplay].ScreenWidth;
                SavedAppsPage.HeightBox.Text = Displays[SelectedDisplay].ScreenHeight;
            }
            catch (Exception)
            {
                SavedAppsPage.XBox.Text = "[Disconnected]";
                SavedAppsPage.YBox.Text = "[Disconnected]";
                SavedAppsPage.WidthBox.Text = "[Disconnected]";
                SavedAppsPage.HeightBox.Text = "[Disconnected]";

            }

        }

        // Clears monitor select button
        public void ResetMonitorSelectButton(object sender, RoutedEventArgs e)
        {
            SavedAppsPage.MonitorSelectButton.Content = " Select\nMonitor";
        }

        // Limits media entry
        public void CmdBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SavedAppsPage.CmdBox.Text == "MEDIA")
            {
                SavedAppsPage.XBox.IsEnabled = false;
                SavedAppsPage.YBox.IsEnabled = false;
                SavedAppsPage.WidthBox.IsEnabled = false;
                SavedAppsPage.HeightBox.IsEnabled = false;
                SavedAppsPage.XBox.Text = Displays[0].MonitorArea.Left.ToString();
                SavedAppsPage.YBox.Text = Displays[0].MonitorArea.Top.ToString();
                SavedAppsPage.WidthBox.Text = Displays[0].ScreenWidth;
                SavedAppsPage.HeightBox.Text = Displays[0].ScreenHeight;
                SavedAppsPage.MonitorSelectButton.Content = "Monitor:\n      1";
            }
            else
            {
                SavedAppsPage.XBox.IsEnabled = true;
                SavedAppsPage.YBox.IsEnabled = true;
                SavedAppsPage.WidthBox.IsEnabled = true;
                SavedAppsPage.HeightBox.IsEnabled = true;
            }
            if (SavedAppsPage.CmdBox.Text == "")
            {
                SavedAppsPage.CmdBox.Text = "Command Line";
            }

        }

        // Tray icon handler thingy
        public void ShowMenuItem_Click(object sender, EventArgs e)
        {
            IntPtr MainHandle = new WindowInteropHelper(this).Handle;
            if (IsHidden == true)
            {
                ShowWindow(MainHandle, 5);
                IsHidden = false;
                ShowMenuItem.Header = "Hide ADP";
            }
            else
            {
                ShowWindow(MainHandle, 0);
                IsHidden = true;
                ShowMenuItem.Header = "Show ADP";
            }
        }

        // Proper close button thingy
        public void CloseMenuItem_Click(object sender, EventArgs e)
        {
            tbi.Visibility = Visibility.Hidden;
            Application.Current.Shutdown();
        }

        // Handles the fix button
        public void FixButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAppsPage.HandleListBox.SelectedItem != null)
            {
                IntPtr hwnd = new IntPtr(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Substring(CurrentAppsPage.HandleListBox.SelectedItem.ToString().Length - 3))][2]));
                FixApp(hwnd);
            }
        }

        // Navigation View Loading Handler Thing™
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            // Set the initial SelectedItem
            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "Page_Settings")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }
            ContentFrame.Navigate(CurrentAppsPage);
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        // Navigation View Selection Handler Thing™
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
        }

        // Navigation View Invoke Handler Thing™
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            {
                if (args.IsSettingsInvoked)
                {
                    ContentFrame.Navigate(SettingsPage);
                }
                else
                {
                    if (args.InvokedItem is TextBlock ItemContent)
                    {
                        switch (ItemContent.Tag)
                        {
                            case "Nav_Current":
                                ContentFrame.Navigate(CurrentAppsPage);
                                break;

                            case "Nav_Saved":
                                ContentFrame.Navigate(SavedAppsPage);
                                break;

                            case "Nav_Help":
                                ContentFrame.Navigate(HelpPage);
                                break;
                        }
                    }
                }
            }
        }


        // /////////////////////////////////////////////////////////////////////////////////////// //
        // //////////// All the weird non-GUIey bits are here don't question it shhhh //////////// //
        // /////////////////////////////////////////////////////////////////////////////////////// //


        // Starts saved apps automatically
        private void StartSavedApps()
        {


            foreach (App i in JSONArrayList)
            {
                if (i.Startup)
                {
                    int t = 1000;
                    if (i.Time != "Wait Time")
                    {
                        try
                        {
                            t = Convert.ToInt32(i.Time);
                        }
                        catch (Exception) { }
                    }

                    if (i.Flags == "Flags")
                    {
                        i.Flags = string.Empty;
                    }
                    WindowFromListToDesktop(i, t);
                }

            }

        }

        // Everything breaks if I remove this
        public IntPtr MainWindowHandle
        {
            get;
        }

        // Super-handy to do things or something
        public class DisplayInfoCollection : List<DisplayInfo>
        {
        }

        // What the above is made of
        public class DisplayInfo
        {
            public string Availability { get; set; }
            public string ScreenHeight { get; set; }
            public string ScreenWidth { get; set; }
            public RECT MonitorArea { get; set; }
            public RECT WorkArea { get; set; }
            public IntPtr Handle { get; set; }
            public int Top { get; set; }
            public int Left { get; set; }
        }

        // Important magical numbers that make windows borderless
        static class WeirdMagicalNumbers
        {
            public const int GWL_STYLE = -16;
            public const int GWL_EXSTYLE = -20;
            public const int SWP_NOSIZE = 0x01;
            public const int SWP_NOMOVE = 0x02;
            public const int SWP_NOZORDER = 0x04;
            public const int SWP_NOACTIVATE = 0x10;
            public const int SWP_NOOWNERZORDER = 0x200;
            public const int SWP_NOSENDCHANGING = 0x400;
            public const int SWP_FRAMECHANGED = 0x20;
            public const int WS_THICKFRAME = 0x40000;
            public const int WS_DLGFRAME = 0x400000;
            public const int WS_BORDER = 0x800000;
            public const int WS_EX_DLGMODALFRAME = 1;
            public const int WS_EX_WINDOWEDGE = 0x100;
            public const int WS_EX_CLIENTEDGE = 0200;
            public const int WS_EX_STATICEDGE = 0x20000;
            public const int SW_SHOWNOACTIVATE = 4;
            public const int SW_RESTORE = 9;
            public const int WM_EXITSIZEMOVE = 0x0232;

        }

        // The thing that defines an app uwu
        public class App
        {
            public string Cmd { get; set; }
            public string Xpos { get; set; }
            public string Ypos { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string Flags { get; set; }
            public string Name { get; set; }
            public string Time { get; set; }
            public bool Lock { get; set; }
            public bool Startup { get; set; }
            public bool Fix { get; set; }
            public bool Pin { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte SystemStatusFlag;
            public uint BatteryLifeTime;
            public uint BatteryFullLifeTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        // Stuff for acquiring mouse position because Cursor.Position failed me
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        // It's like a normal bool but delegate, perhaps its also delicate? I don't know. That's up to you, I suppose!
        public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        // Gets a list of display info
        public DisplayInfoCollection GetDisplays()
        {
            DisplayInfoCollection col = new DisplayInfoCollection();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
                {
                    MONITORINFO mi = new MONITORINFO();
                    mi.cbSize = (uint)Marshal.SizeOf(mi);
                    bool success = GetMonitorInfo(hMonitor, ref mi);
                    if (success)
                    {
                        DisplayInfo di = new DisplayInfo
                        {
                            ScreenWidth = (mi.rcMonitor.Right - mi.rcMonitor.Left).ToString(),
                            ScreenHeight = (mi.rcMonitor.Bottom - mi.rcMonitor.Top).ToString(),
                            MonitorArea = mi.rcMonitor,
                            WorkArea = mi.rcWork,
                            Availability = mi.dwFlags.ToString(),
                            Handle = hMonitor,
                            Top = mi.rcMonitor.Top,
                            Left = mi.rcMonitor.Left
                        };
                        col.Add(di);
                    }
                    return true;
                }, IntPtr.Zero);
            return col;
        }

        // Deals with getting a window's size
        public static Size GetWindowSize(IntPtr hWnd)
        {
            Size cSize = new Size();
            // get coordinates relative to window
            GetWindowRect(hWnd, out RECT pRect);

            cSize.Width = pRect.Right - pRect.Left;
            cSize.Height = pRect.Bottom - pRect.Top;
            return cSize;
        }

        // Have a guess what this does you fkn idiot
        public static Point GetCursorPosition()
        {
            GetCursorPos(out POINT lpPoint);
            return lpPoint;
        }

        // idk tbh
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        // Get the pare- no, child windows
        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> AllActiveHandles = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(AllActiveHandles);
            try
            {
                EnumWindowsProc childProc = new EnumWindowsProc(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return AllActiveHandles;
        }

        // Something to do with enumeration, not that I actually understand it
        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            if (!(gch.Target is List<IntPtr> list))
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            return true;
        }

        // Actually removes the borders
        public void RemoveBorders(int TargetHandlePtr)
        {
            IntPtr TargetHandle = new IntPtr(TargetHandlePtr);
            int nStyle = GetWindowLong(TargetHandle, WeirdMagicalNumbers.GWL_STYLE);
            nStyle = (nStyle | (WeirdMagicalNumbers.WS_THICKFRAME + WeirdMagicalNumbers.WS_DLGFRAME + WeirdMagicalNumbers.WS_BORDER)) ^ (WeirdMagicalNumbers.WS_THICKFRAME + WeirdMagicalNumbers.WS_DLGFRAME + WeirdMagicalNumbers.WS_BORDER);
            SetWindowLong(TargetHandle, WeirdMagicalNumbers.GWL_STYLE, nStyle);

            nStyle = GetWindowLong(TargetHandle, WeirdMagicalNumbers.GWL_EXSTYLE);
            nStyle = (nStyle | (WeirdMagicalNumbers.WS_EX_DLGMODALFRAME + WeirdMagicalNumbers.WS_EX_WINDOWEDGE + WeirdMagicalNumbers.WS_EX_CLIENTEDGE + WeirdMagicalNumbers.WS_EX_STATICEDGE)) ^ (WeirdMagicalNumbers.WS_EX_DLGMODALFRAME + WeirdMagicalNumbers.WS_EX_WINDOWEDGE + WeirdMagicalNumbers.WS_EX_CLIENTEDGE + WeirdMagicalNumbers.WS_EX_STATICEDGE);
            SetWindowLong(TargetHandle, WeirdMagicalNumbers.GWL_EXSTYLE, nStyle);

            int uFlags = WeirdMagicalNumbers.SWP_NOSIZE | WeirdMagicalNumbers.SWP_NOMOVE | WeirdMagicalNumbers.SWP_NOZORDER | WeirdMagicalNumbers.SWP_NOACTIVATE | WeirdMagicalNumbers.SWP_NOOWNERZORDER | WeirdMagicalNumbers.SWP_NOSENDCHANGING | WeirdMagicalNumbers.SWP_FRAMECHANGED;
            SetWindowPos(TargetHandlePtr, 0, 0, 0, 0, 0, uFlags);
        }

        // Actually adds the borders
        public void AddBorders(int SelectedHandlePtr, int nStyle, int nStyleEx)
        {
            IntPtr SelectedHandle = new IntPtr(SelectedHandlePtr);
            SetWindowLong(SelectedHandle, WeirdMagicalNumbers.GWL_STYLE, nStyle);

            SetWindowLong(SelectedHandle, WeirdMagicalNumbers.GWL_EXSTYLE, nStyleEx);

            int uFlags = WeirdMagicalNumbers.SWP_NOSIZE | WeirdMagicalNumbers.SWP_NOMOVE | WeirdMagicalNumbers.SWP_NOZORDER | WeirdMagicalNumbers.SWP_NOACTIVATE | WeirdMagicalNumbers.SWP_NOOWNERZORDER | WeirdMagicalNumbers.SWP_NOSENDCHANGING | WeirdMagicalNumbers.SWP_FRAMECHANGED;
            SetWindowPos(SelectedHandlePtr, 0, 0, 0, 0, 0, uFlags);
        }

        // Checks for AppData directory/configs and creates them if they doesn't exist
        private void FileSystem()
        {
            string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            LocalFolder = Path.Combine(AppData, "ActiveDesktopPlus");

            Directory.CreateDirectory(LocalFolder);


            if (!System.IO.File.Exists(Path.Combine(LocalFolder, "saved.json")))
            {
                WriteJSON(); // butts - Missy Quarry, 2020
            }
            if (System.IO.File.ReadAllText(Path.Combine(LocalFolder, "saved.json")) == "" || System.IO.File.ReadAllText(Path.Combine(LocalFolder, "saved.json")) == null)
            {
                WriteJSON(); // Initialises empty file
            }
            
        }

        // Writes stuff in the array to the JSON file
        private void WriteJSON()
        {
            System.IO.File.Create(Path.Combine(LocalFolder, "saved.json")).Close();
            System.IO.File.WriteAllText(Path.Combine(LocalFolder, "saved.json"), JsonConvert.SerializeObject(JSONArrayList, Formatting.Indented));
        }

        // Reads stuff from the JSON file into the array
        public List<App> ReadJSON()
        {
            string JsonAppList = System.IO.File.ReadAllText(Path.Combine(LocalFolder, "saved.json"));
            List<App> al = JsonConvert.DeserializeObject<List<App>>(JsonAppList);
            return al;
        }

        // Locks an app for real this time
        private void LockApp(IntPtr hwnd)
        {
            if (EnableWindow(hwnd, false))
            {
                EnableWindow(hwnd, true);
            }
        }

        // Fixes an app for real this time
        private void FixApp(IntPtr hwnd)
        {
            RECT WinPos = new RECT();
            GetWindowRect(hwnd, out WinPos);
            MoveWindow(hwnd, TranslateCanvasX(TranslateCanvasX(WinPos.Left)), TranslateCanvasY(TranslateCanvasY(WinPos.Top)), WinPos.Right - WinPos.Left, WinPos.Bottom - WinPos.Top, true);
        }

        // Pins an app for real this time
        private void PinApp(string hwnd)
        {
            RefreshLists();
            foreach (List<string> l in DesktopWindowPropertyList)
            {
                if (l[2] == hwnd && l[3] == "false")
                {
                    RemoveBorders(Convert.ToInt32(l[2]));
                    l[3] = "true";
                }
                else if (l[2] == hwnd)
                {
                    try
                    {
                        AddBorders(Convert.ToInt32(l[2]), RetrieveWindowProperties(Convert.ToInt32(l[2]), 0), RetrieveWindowProperties(Convert.ToInt32(l[2]), 1));
                        l[3] = "false";
                    }
                    catch (Exception) { }
                }
            }
            RefreshLists();
        }

        // Actually deals with window properties or smth idk
        private void WindowFromListToDesktop(App i, int t)
        {
            if (SavedAppsPage.XBox.Text == "[Disconnected]")
            {
                SavedAppsPage.XBox.Text = Displays[0].MonitorArea.Left.ToString();
                SavedAppsPage.YBox.Text = Displays[0].MonitorArea.Top.ToString();
                SavedAppsPage.WidthBox.Text = Displays[0].ScreenWidth;
                SavedAppsPage.HeightBox.Text = Displays[0].ScreenHeight;
            }
            if (i.Cmd != "MEDIA")
            {
                Process SavedProcess = Process.Start(i.Cmd, i.Flags);
                SavedProcess.Refresh();
                Thread.Sleep(t);
                SetWindowSizeAndLock(i, SavedProcess.MainWindowHandle);
            }
            else
            {
                ADPVideoWallpaper GeneratedVideoWallpaper = new ADPVideoWallpaper(i.Flags);
                IntPtr hvid = new WindowInteropHelper(GeneratedVideoWallpaper).Handle;
                Thread.Sleep(Convert.ToInt32(t));
                try
                {
                    SetWindowSizeAndLock(i, hvid);
                }
                catch (Exception) { }
            }
        }

        // WindowFromListToDesktop 2 Electric Boogaloo
        public void SetWindowSizeAndLock(App i, IntPtr hwnd)
        {
            try
            {
                SetParent(hwnd, DesktopHandle);
                GetWindowRect(hwnd, out RECT PosTarget);
                if (i.Xpos == "X")
                {
                    i.Xpos = PosTarget.Top.ToString();
                }
                if (i.Ypos == "Y")
                {
                    i.Ypos = PosTarget.Left.ToString();
                }
                if (i.Width == "Width")
                {
                    i.Width = GetWindowSize(hwnd).Width.ToString();
                }
                if (i.Height == "Height")
                {
                    i.Height = GetWindowSize(hwnd).Height.ToString();
                }

                int CorrectedXpos = TranslateCanvasX(Convert.ToInt32(i.Xpos));
                int CorrectedYpos = TranslateCanvasY(Convert.ToInt32(i.Ypos));

                MoveWindow(hwnd, CorrectedXpos, CorrectedYpos, Convert.ToInt32(i.Width), Convert.ToInt32(i.Height), true);
                if (i.Lock)
                {
                    LockApp(hwnd);
                }
                if (i.Fix)
                {
                    Thread.Sleep(100);
                    FixApp(hwnd);
                }
                if (i.Pin)
                {
                    PinApp(hwnd.ToString());
                }
            }
            catch (Exception) { }
        }

        // Deals with misaligned desktop/monitor origins for the X axis.
        public int TranslateCanvasX(int x)
        {
            int LargestNegative = 0;
            foreach (DisplayInfo Display in Displays)
            {
                if (Display.Left < LargestNegative)
                {
                    LargestNegative = Display.Left;
                }
            }
            return x - LargestNegative;
        }

        // Deals with misaligned desktop/monitor origins for the Y axis.
        public int TranslateCanvasY(int y)
        {
            int LargestNegative = 0;
            foreach (DisplayInfo Display in Displays)
            {
                if (Display.Top < LargestNegative)
                {
                    LargestNegative = Display.Top;
                }
            }
            return y - LargestNegative;
        }

        // Manages minimising to the tray.
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            IntPtr MainHandle = new WindowInteropHelper(this).Handle;
            ShowWindow(MainHandle, 0);
            ShowWindow(MainHandle, 0);
            IsHidden = true;
            ShowMenuItem.Header = "Show ADP";
        }

        // Something to do with getting monitors (hahalol like I understand it)
        public bool MonitorEnumProc(IntPtr MonitorHandle, IntPtr hdc, out RECT UnusedButNecessaryRECT, IntPtr UnusedButNecessaryIntPtr)
        {
            UnusedButNecessaryRECT = new RECT();
            return true;
        }

        // Weird cursed external stuff that terrifies me
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowExA(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(int xPoint, int yPoint);
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndParent);
        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, IntPtr i);
        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint QueryFullProcessImageNameW(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint nSize);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheretHandle, uint dwProcessId);
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lplmi);
        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hwnd);
        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromRect(in RECT lprc, uint dwFlags);
        [DllImport("user32.dll")]
        public static extern bool PtInRect(in RECT lprc, POINT pt);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
        [DllImport("kernel32.dll")]
        public static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);
    }

}
