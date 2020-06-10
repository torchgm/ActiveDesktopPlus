﻿using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IWshRuntimeLibrary;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;

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
        List<List<string>> WindowList; // Not entirely sure, probably something to do with the children of the desktop
        List<int> WindowHandles = new List<int>(); // List of handles
        List<int> WindowProperties = new List<int>(); // List of window properties
        List<int> WindowPropertiesEx = new List<int>(); // List of window properties but this time its ex


        // On-start events
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
            HwndInputTextBox.Text = DesktopHandle.ToString();

            // Trigger a refresh of many things. Not strictly necessary for all of this but hey extra refreshing is always nice
            FileSystem();
            JSONArrayList = ReadJSON();
            if (JSONArrayList.Count != 0 && WindowHandles.Count() == 0)
            {
                StartSavedApps();
            }
            RefreshLists();

            if (!System.IO.File.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Active Desktop Plus.lnk")))
            {
                StartupCheckBox.IsChecked = false;
            }
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

        // Deals with listening to the B key
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            // Deals with finding the target window the user wants to send to the desktop
            // I'll probably work out how to do it with the mouse at some point but 🅱
            if (e.Key == Key.B)
            {
                TargetHandle = WindowFromPoint(Convert.ToInt32(GetCursorPosition().X), Convert.ToInt32(GetCursorPosition().Y));
                HwndInputTextBox.Text = TargetHandle.ToString();
                StringBuilder WindowTitle = new StringBuilder(1000);
                int result = GetWindowText(TargetHandle, WindowTitle, 1000);
                TitleTextBox.Text = WindowTitle.ToString();
            }
        }

        // Sends the selected handle to the desktop
        private void ApplyHwndButton_Click(object sender, RoutedEventArgs e)
        {
            // This bit just makes the window a child of the desktop. Honestly a lot easier than I first thought.
            SetParent(TargetHandle, DesktopHandle);
            RefreshLists();
        }

        // Makes the selected window borderless
        private void BorderlessButton_Click(object sender, RoutedEventArgs e)
        { // This long potato does some mighty magic that takes the ID and gets the handle from the WindowList thing that I made above ^
            if (HandleListBox.SelectedItem != null)
            {
                RemoveBorders(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]));
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
                    AddBorders(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]), RetrieveWindowProperties(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]), 0), RetrieveWindowProperties(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]), 1));
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
            WindowList = new List<List<string>>();

            // Checks every child of the desktop to see if they're a direct root window, then if they are, assigns them a numerical ID and adds their title and handle to a list.
            HandleListBox.Items.Clear();
            for (IntPtr ChildHandle = FindWindowExA(DesktopHandle, IntPtr.Zero, null, null); ChildHandle != IntPtr.Zero; ChildHandle = FindWindowExA(DesktopHandle, ChildHandle, null, null))
            {
                int result = GetWindowText(ChildHandle, WindowTitle, 1000);
                if (result != 1001 && WindowTitle.ToString() != "FolderView")
                {
                    // This bit is the clever bit that creates an entry for every window on the desktop
                    WindowList.Add(new List<string>());
                    WindowList[count].Add((1000 + count).ToString()); // ID - Nobody is ever going to have more than 1000 windows open, if they do this will break but hey idc
                    WindowList[count].Add(WindowTitle.ToString()); // Window Title
                    WindowList[count].Add(ChildHandle.ToString()); // Window Handle
                    StoreWindowProperties(ChildHandle.ToInt32(), (int)GetWindowLong(ChildHandle, WeirdMagicalNumbers.GWL_STYLE), (int)GetWindowLong(ChildHandle, WeirdMagicalNumbers.GWL_EXSTYLE));
                    HandleListBox.Items.Add(WindowList[count][1] + " " + WindowList[count][0]);
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

                IntPtr hwnd = new IntPtr(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]));
                LockApp(hwnd);
            }
        }

        // Allows the user to easily save the selected window
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            RECT PosTarget;
            uint PID;
            string SyllyIsAwesome;
            IntPtr hwnd = new IntPtr(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]));
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

        // Attempts to close the selected app
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (HandleListBox.SelectedItem != null)
            {
                IntPtr hwnd = new IntPtr(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]));
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
                //Create First Shortcut for Application Settings
                string ADPStartupLink = System.IO.Path.Combine(shortcutFolder, "Active Desktop Plus.lnk");
                IWshShortcut shortcut = (IWshShortcut)shellClass.CreateShortcut(ADPStartupLink);
                shortcut.TargetPath = System.Environment.GetCommandLineArgs()[0];
                shortcut.IconLocation = System.Environment.GetCommandLineArgs()[0];
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
                string ADPStartupLink = System.IO.Path.Combine(shortcutFolder, "Active Desktop Plus.lnk");
                System.IO.File.Delete(ADPStartupLink);
            }
            catch (Exception)
            {

                throw;
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

        // Everything breaks if I remove this
        public IntPtr MainWindowHandle
        {
            get;
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

            LocalFolder = System.IO.Path.Combine(AppData, "ActiveDesktopPlus");

            Directory.CreateDirectory(LocalFolder);


            if (!System.IO.File.Exists(System.IO.Path.Combine(LocalFolder, "saved.json")))
            {
                FileStream fs = System.IO.File.Create(System.IO.Path.Combine(LocalFolder, "saved.json")); // butts - Missy Quarry, 2020
                fs.Dispose();
            }
        }

        // Writes stuff in the array to the JSON file
        private void WriteJSON()
        {
            System.IO.File.Create(System.IO.Path.Combine(LocalFolder, "saved.json")).Close();
            System.IO.File.WriteAllText(System.IO.Path.Combine(LocalFolder, "saved.json"), JsonConvert.SerializeObject(JSONArrayList, Formatting.Indented));
        }

        // Reads stuff from the JSON file into the array
        public List<App> ReadJSON()
        {
            string JsonAppList = System.IO.File.ReadAllText(System.IO.Path.Combine(LocalFolder, "saved.json"));
            List<App> al = JsonConvert.DeserializeObject<List<App>>(JsonAppList);
            return al;
        }

        // Locks an app for real this time
        private void LockApp(IntPtr hwnd)
        {
            StringBuilder WindowTitle = new StringBuilder(1000);
            LockWindow GeneratedLockWindow = new LockWindow();
            GetWindowText(hwnd, WindowTitle, 1000);
            GeneratedLockWindow.Title = "LockWindow For " + WindowTitle;
            GeneratedLockWindow.Show();
            IntPtr hlock = new WindowInteropHelper(GeneratedLockWindow).Handle;
            RECT WindowTargetLock;
            GetWindowRect(hwnd, out WindowTargetLock);
            GeneratedLockWindow.Top = WindowTargetLock.Top;
            GeneratedLockWindow.Left = WindowTargetLock.Left;
            GeneratedLockWindow.Width = GetWindowSize(hwnd).Width;
            GeneratedLockWindow.Height = GetWindowSize(hwnd).Height;
            Thread.Sleep(500);
            SetParent(hlock, DesktopHandle);
            Thread.Sleep(250);
            RefreshLists();
        }












        private void WindowFromListToDesktop(App i, int t)
        {
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
                GeneratedVideoWallpaper.Show();
                IntPtr hvid = new WindowInteropHelper(GeneratedVideoWallpaper).Handle;
                Thread.Sleep(Convert.ToInt32(t));
                SetParent(hvid, DesktopHandle);
                GeneratedVideoWallpaper.WindowState = WindowState.Maximized;
                SetWindowSizeAndLock(i, hvid);
            }
        }

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

                MoveWindow(hwnd, Convert.ToInt32(i.Xpos), Convert.ToInt32(i.Ypos), Convert.ToInt32(i.Width), Convert.ToInt32(i.Height), true);
                if (i.Lock)
                {
                    LockApp(hwnd);
                }
            }
            catch (Exception) { }
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
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
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
    }
}
