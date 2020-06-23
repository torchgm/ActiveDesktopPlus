using IWshRuntimeLibrary;
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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using PInvoke;

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

        // On-start tasks
        public MainWindow()
        {
            InitializeComponent();
            
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
            if (JSONArrayList.Count != 0 && WindowHandles.Count() == 0)
            {
                StartSavedApps();
            }
            RefreshLists();

            if (!System.IO.File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Active Desktop Plus.lnk")))
            {
                StartupCheckBox.IsChecked = false;
            }
            Displays = GetDisplays();
            // Disables Lock button on multi-monitor setups (as it doesn't work reliably)
            if (Displays.Count > 1)
            {
                LockButton.IsEnabled = false;
                LockedCheckBox.IsEnabled = false;
            }
            //Show();
            FixOriginScalingWithVideoWallpaperBecauseItDoesntWorkOtherwise();

            TitleTextBox.Text = "[Hold Ctrl to select an app]";
            HwndInputTextBox.Text = "";
        }

        // This exists because for some insanely stupid reason, scaling simply doesn't work unless a video is played across all desktops first
        public void FixOriginScalingWithVideoWallpaperBecauseItDoesntWorkOtherwise()
        {
            ADPVideoWallpaper TempVideoWallpaper = new ADPVideoWallpaper(@"C:\.mp4");
            IntPtr hvid = new WindowInteropHelper(TempVideoWallpaper).Handle;
            Thread.Sleep(20);
            SetParent(hvid, DesktopHandle);
            foreach (DisplayInfo di in Displays)
            {
                MoveWindow(hvid, TranslateCanvasX(di.Top), TranslateCanvasY(di.Top), Convert.ToInt32(di.ScreenWidth), Convert.ToInt32(di.ScreenHeight), true);
                Thread.Sleep(20);
            }
            Thread.Sleep(20);
            DestroyWindow(hvid);
            RefreshLists();
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
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            // Deals with finding the target window the user wants to send to the desktop
            // I'll probably work out how to do it with the mouse at some point but 🅱 was easier at the time
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                TargetHandle = WindowFromPoint(Convert.ToInt32(GetCursorPosition().X), Convert.ToInt32(GetCursorPosition().Y));
                HwndInputTextBox.Text = TargetHandle.ToString();
                StringBuilder WindowTitle = new StringBuilder(1000);
                int result = GetWindowText(TargetHandle, WindowTitle, 1000);
                TitleTextBox.Text = WindowTitle.ToString();
                if (HwndInputTextBox.Text == DesktopHandle.ToString())
                {
                    TitleTextBox.Text = "[Desktop]";
                }
                else if (TitleTextBox.Text == "")
                {
                    TitleTextBox.Text = "[Window has no title]";
                }
                ApplyHwndButton.Content = "Hover over an app's title bar to select it, then release Ctrl";
                ApplyHwndButton.IsEnabled = false;
            }
        }

        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            ApplyHwndButton.Content = "Send to Desktop";
            ApplyHwndButton.IsEnabled = true;
        }

        // Sends the selected handle to the desktop
        private void ApplyHwndButton_Click(object sender, RoutedEventArgs e)
        {
            // This bit just makes the window a child of the desktop. Honestly a lot easier than I first thought.
            RECT TempRect = new RECT();
            GetWindowRect(TargetHandle, out TempRect);
            SetParent(TargetHandle, DesktopHandle);
            int x = TranslateCanvasX(0);
            int y = TranslateCanvasY(0);
            MoveWindow(TargetHandle, (TempRect.Left + x), (TempRect.Top + y), Convert.ToInt32(GetWindowSize(TargetHandle).Width), Convert.ToInt32(GetWindowSize(TargetHandle).Height), true);
            TitleTextBox.Text = "[Hold Ctrl to select an app]";
            HwndInputTextBox.Text = "";
            
            RefreshLists();
        }

        protected override void OnActivated(EventArgs e)
        {
            RefreshLists();
            base.OnActivated(e);
        }

        // Makes the selected window borderless
        private void BorderlessButton_Click(object sender, RoutedEventArgs e)
        { // This long potato does some mighty magic that takes the ID and gets the handle from the WindowList thing that I made above ^
            if (HandleListBox.SelectedItem != null)
            {
                RemoveBorders(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]));
            }
            RefreshLists();
        }

        // Makes the selected window bordered
        private void UnborderlessButton_Click(object sender, RoutedEventArgs e)
        {
            // Yes I am well-aware this line is insanely long and could be shorter, and that catching everything is an awful idea. It is never going to be updated though because I know it annoys Sylly and that's cute.
            if (HandleListBox.SelectedItem != null)
            {
                try
                {
                    AddBorders(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]), RetrieveWindowProperties(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]), 0), RetrieveWindowProperties(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]), 1));
                }
                catch (Exception) { }
            }
            RefreshLists();
        }

        // Refresh event for children of the desktop
        public void RefreshLists()
        {
            // Quite frankly I've forgotten how this bit works but it needs to be a StringBuilder and I just hope window titles aren't too long
            StringBuilder WindowTitle = new StringBuilder(1000);
            int count = 0;
            DesktopWindowPropertyList = new List<List<string>>();

            // Checks every child of the desktop to see if they're a direct root window, then if they are, assigns them a numerical ID and adds their title and handle to a list.
            HandleListBox.Items.Clear();
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
                    HandleListBox.Items.Add(DesktopWindowPropertyList[count][1] + " " + DesktopWindowPropertyList[count][0]);
                    count++;

                }
            }
            SavedListRefreshEvent();
        }

        // Refresh event for the saved apps list
        private void SavedListRefreshEvent()
        {
            SavedListBox.Items.Clear();
            foreach (App i in JSONArrayList)
            {
                if (i.Name == "Friendly Name")
                {
                    SavedListBox.Items.Add(i.Cmd);

                }
                else
                {
                    SavedListBox.Items.Add(i.Name);
                }
            }
        }

        // Handles adding a new app to the saved apps list
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (CmdBox.Text != "Command Line")
            {
                App AppToAdd = new App();
                AppToAdd.Cmd = CmdBox.Text;
                AppToAdd.Xpos = XBox.Text;
                AppToAdd.Ypos = YBox.Text;
                AppToAdd.Width = WidthBox.Text;
                AppToAdd.Height = HeightBox.Text;
                AppToAdd.Flags = FlagBox.Text;
                AppToAdd.Name = NameBox.Text;
                AppToAdd.Time = TimeBox.Text;
                AppToAdd.Lock = LockedCheckBox.IsChecked ?? false;
                AppToAdd.Startup = AutostartCheckBox.IsChecked ?? false;
                JSONArrayList.Add(AppToAdd);



                CmdBox.Text = "Command Line";
                XBox.Text = "X";
                YBox.Text = "Y";
                WidthBox.Text = "Width";
                HeightBox.Text = "Height";
                FlagBox.Text = "Flags";
                NameBox.Text = "Friendly Name";
                TimeBox.Text = "Wait Time";
                WriteButton.IsEnabled = true;
            }
            SavedListRefreshEvent();
        }

        // Calls a refresh of the children of the desktop
        private void AppList_Click(object sender, MouseButtonEventArgs e)
        {
            RefreshLists();
        }

        // Button that removes entries from the saved app list
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedListBox.SelectedIndex != -1)
            {
                JSONArrayList.RemoveAt(SavedListBox.SelectedIndex);
                SavedListRefreshEvent();
                WriteButton.IsEnabled = true;
            }

        }

        // Button that writes changes to disk
        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            WriteJSON();
            WriteButton.IsEnabled = false;
        }

        // Button that tests how an application behaves on startup
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedListBox.SelectedIndex != -1)
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

                    if (n == SavedListBox.SelectedIndex)
                    {
                        WindowFromListToDesktop(i, t);
                    }


                    ++n;
                }
            }
            RefreshLists();
        }

        // Draws a locking window over any given app
        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            if (HandleListBox.SelectedItem != null)
            {

                IntPtr hwnd = new IntPtr(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]));
                LockApp(hwnd);
            }
        }

        // Allows the user to easily save the selected window
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (HandleListBox.SelectedIndex != -1)
            {
                RECT PosTarget;
                uint PID;
                string SyllyIsAwesome;
                IntPtr hwnd = IntPtr.Zero;
                hwnd = new IntPtr(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]));
                StringBuilder WindowTitle = new StringBuilder(1000);
                StringBuilder FileName = new StringBuilder(1000);
                uint size = (uint)FileName.Capacity;
                GetWindowThreadProcessId(hwnd, out PID);
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

                GetWindowRect(hwnd, out PosTarget);
                GetWindowText(hwnd, WindowTitle, 1000);
                CmdBox.Text = SyllyIsAwesome;
                XBox.Text = PosTarget.Top.ToString();
                YBox.Text = PosTarget.Left.ToString();
                WidthBox.Text = GetWindowSize(hwnd).Width.ToString();
                HeightBox.Text = GetWindowSize(hwnd).Height.ToString();
                NameBox.Text = WindowTitle.ToString();
            }
        }

        // Attempts to close the selected app
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (HandleListBox.SelectedItem != null)
            {
                IntPtr hwnd = new IntPtr(Convert.ToInt32(DesktopWindowPropertyList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]));
                uint WM_CLOSE = 0x0010;
                SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            Thread.Sleep(250);
            RefreshLists();
        }

        // Adds startup shortcut
        private void StartupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                string shortcutFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (!Directory.Exists(shortcutFolder))
                {
                    Directory.CreateDirectory(shortcutFolder);
                }
                WshShellClass shellClass = new WshShellClass();
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
        private void StartupCheckBox_Unchecked(object sender, RoutedEventArgs e)
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
        private void MonitorSelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedDisplay++;
            if (SelectedDisplay > Displays.Count)
            {
                SelectedDisplay = 0;
            }
            MonitorSelectButton.Content = "Monitor: " + (SelectedDisplay + 1).ToString();
            try
            {
                XBox.Text = Displays[SelectedDisplay].MonitorArea.Left.ToString();
                YBox.Text = Displays[SelectedDisplay].MonitorArea.Top.ToString();
                WidthBox.Text = Displays[SelectedDisplay].ScreenWidth;
                HeightBox.Text = Displays[SelectedDisplay].ScreenHeight;
            }
            catch (Exception)
            {
                XBox.Text = "[Disconnected]";
                YBox.Text = "[Disconnected]";
                WidthBox.Text = "[Disconnected]";
                HeightBox.Text = "[Disconnected]";

            }

        }

        // Clears monitor select button
        private void ResetMonitorSelectButton(object sender, RoutedEventArgs e)
        {
            MonitorSelectButton.Content = " Select\nMonitor";
        }
        
        // Limits media entry
        private void CmdBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CmdBox.Text == "MEDIA")
            {
                XBox.IsEnabled = false;
                YBox.IsEnabled = false;
                WidthBox.IsEnabled = false;
                HeightBox.IsEnabled = false;
                MonitorSelectButton_Click(null, null);
            }
            else
            {
                XBox.IsEnabled = true;
                YBox.IsEnabled = true;
                WidthBox.IsEnabled = true;
                HeightBox.IsEnabled = true;
            }
            if (CmdBox.Text == "")
            {
                CmdBox.Text = "Command Line";
            }

        }

        // Tray icon handler thingy
        private void ShowMenuItem_Click(object sender, EventArgs e)
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
        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
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
                        DisplayInfo di = new DisplayInfo();
                        di.ScreenWidth = (mi.rcMonitor.Right - mi.rcMonitor.Left).ToString();
                        di.ScreenHeight = (mi.rcMonitor.Bottom - mi.rcMonitor.Top).ToString();
                        di.MonitorArea = mi.rcMonitor;
                        di.WorkArea = mi.rcWork;
                        di.Availability = mi.dwFlags.ToString();
                        di.Handle = hMonitor;
                        di.Top = mi.rcMonitor.Top;
                        di.Left = mi.rcMonitor.Left;
                        col.Add(di);
                    }
                    return true;
                }, IntPtr.Zero);
            return col;
        }

        // Deals with getting a window's size
        public static Size GetWindowSize(IntPtr hWnd)
        {
            RECT pRect;
            Size cSize = new Size();
            // get coordinates relative to window
            GetWindowRect(hWnd, out pRect);

            cSize.Width = pRect.Right - pRect.Left;
            cSize.Height = pRect.Bottom - pRect.Top;
            return cSize;
        }

        // Have a guess what this does you fkn idiot
        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
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
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
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

        // Checks for AppData directory/CSV and creates it if it doesn't exist
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
                WriteJSON(); // initialises empty files
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
            StringBuilder WindowTitle = new StringBuilder(1000);
            GetWindowText(hwnd, WindowTitle, 1000);

            LockWindow GeneratedLockWindow = new LockWindow();
            GeneratedLockWindow.Title = "LockWindow For " + WindowTitle;
            GeneratedLockWindow.Show();

            IntPtr hlock = new WindowInteropHelper(GeneratedLockWindow).Handle;
            
            RECT WindowTargetLock;
            GetWindowRect(hwnd, out WindowTargetLock);

            GeneratedLockWindow.Width = GetWindowSize(hwnd).Width;
            GeneratedLockWindow.Height = GetWindowSize(hwnd).Height;
            GeneratedLockWindow.Left = TranslateCanvasX(WindowTargetLock.Left);
            GeneratedLockWindow.Top = TranslateCanvasY(WindowTargetLock.Top);

            // Technically just the same as above, keeping so i don't forget
            //int CorrectedXpos = TranslateCanvasX(Convert.ToInt32(WindowTargetLock.Left));
            //int CorrectedYpos = TranslateCanvasY(Convert.ToInt32(WindowTargetLock.Top));
            //MoveWindow(hwnd, CorrectedXpos - 1, CorrectedYpos, Convert.ToInt32(GetWindowSize(hwnd).Width), Convert.ToInt32(GetWindowSize(hwnd).Height), true);
            SetParent(hlock, DesktopHandle);

            RefreshLists();
        }

        // Actually deals with window properties or smth idk
        private void WindowFromListToDesktop(App i, int t)
        {
            if (XBox.Text == "[Disconnected]")
            {
                XBox.Text = Displays[0].MonitorArea.Left.ToString();
                YBox.Text = Displays[0].MonitorArea.Top.ToString();
                WidthBox.Text = Displays[0].ScreenWidth;
                HeightBox.Text = Displays[0].ScreenHeight;
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
                SetParent(hvid, DesktopHandle);
                //GeneratedVideoWallpaper.WindowState = WindowState.Maximized;
                SetWindowSizeAndLock(i, hvid);
            }
        }

        // WindowFromListToDesktop 2 Electric Boogaloo
        public void SetWindowSizeAndLock(App i, IntPtr hwnd)
        {
            try
            {
                SetParent(hwnd, DesktopHandle);
                RECT PosTarget;
                GetWindowRect(hwnd, out PosTarget);
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

        private void MediaButton_Click(object sender, RoutedEventArgs e)
        {
            CmdBox.Text = "MEDIA";
            FlagBox.Text = "Path to Video";
        }

        private void CmdBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CmdBox.Text == "MEDIA" || CmdBox.Text == "Command Line")
            {
                CmdBox.Text = "";
                FlagBox.Text = "Flags";
            }
        }

        private void FlagBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FlagBox.Text == "")
            {
                if (CmdBox.Text == "MEDIA")
                {
                    FlagBox.Text = "Path to Video";
                }
                else
                {
                    FlagBox.Text = "Flags";
                }
            }
        }


        // Literally all these do is emulate the behaviour of a watermark in the TextBoxes because WPF really sucks so that's why they're all down here alone.
        private void FlagBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FlagBox.Text == "Path to Video" || FlagBox.Text == "Flags")
            {
                FlagBox.Text = "";
            }
        }

        private void XBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (XBox.Text == "")
            {
                XBox.Text = "X";
            }
        }

        private void XBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (XBox.Text == "X")
            {
                XBox.Text = "";
            }
        }

        private void YBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (YBox.Text == "")
            {
                YBox.Text = "Y";
            }
        }

        private void YBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (YBox.Text == "Y")
            {
                YBox.Text = "";
            }
        }

        private void WidthBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (WidthBox.Text == "")
            {
                WidthBox.Text = "Width";
            }
        }

        private void WidthBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (WidthBox.Text == "Width")
            {
                WidthBox.Text = "";
            }
        }

        private void HeightBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HeightBox.Text == "")
            {
                HeightBox.Text = "Height";
            }
        }

        private void HeightBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (HeightBox.Text == "Height")
            {
                HeightBox.Text = "";
            }
        }

        private void NameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NameBox.Text == "")
            {
                NameBox.Text = "Friendly Name";
            }
        }

        private void NameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NameBox.Text == "Friendly Name")
            {
                NameBox.Text = "";
            }
        }

        private void TimeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TimeBox.Text == "")
            {
                TimeBox.Text = "Wait Time";
            }
        }

        private void TimeBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TimeBox.Text == "Wait Time")
            {
                TimeBox.Text = "";
            }
        }
    }

}
