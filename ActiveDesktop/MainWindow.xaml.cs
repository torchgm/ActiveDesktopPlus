using System;
using System.IO;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace ActiveDesktop
{
    public partial class MainWindow : Window
    {
        // I'm sure it's bad practice to ever declare anything up here ever but screw that I'm doing it anyway
        // It's easier and I need them everywhere so shh it'll be fine I promise
        IntPtr DesktopHandle;
        IntPtr TargetHandle;
        string LocalFolder;
        string[][] CSVArray;
        List<List<string>> WindowList;
        List<int> WindowHandles = new List<int>();
        List<uint> WindowProperties = new List<uint>();
        List<uint> WindowPropertiesEx = new List<uint>();
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

            // Trigger a refresh of the pinned window list. Not strictly necessary but hey extra refreshing is always nice
            RefreshButton_Click(null, null);
            FileSystem();

            if (File.ReadAllText(System.IO.Path.Combine(LocalFolder, "saved.csv")) == string.Empty)
            {
                CSVArray = ReadCSV();
            }
        }

        public void StoreWindowProperties(int Handle, uint Properties, uint PropertiesEx)
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

        public uint RetrieveWindowProperties(int Handle, int Ex)
        {
            for (int i = 0; i < WindowHandles.Count; i++)
            {
                if (WindowHandles[i] == Handle && Ex == 1)
                {
                    return WindowPropertiesEx[i];
                }
                else if (WindowHandles[i] == Handle)
                {
                    return WindowProperties[i];
                }
            }
            return 0;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            // Deals with finding the target window the user wants to send to the desktop
            // I'll probably work out how to do it with the mouse at some point but 🅱
            if (e.Key == Key.B)
            {
                TargetHandle = WindowFromPoint(Convert.ToInt32(GetCursorPosition().X), Convert.ToInt32(GetCursorPosition().Y));
                HwndInputTextBox.Text = TargetHandle.ToString();
            }
        }

        private void ApplyHwndButton_Click(object sender, RoutedEventArgs e)
        {
            // This bit just makes the window a child of the desktop. Honestly a lot easier than I first thought.
            SetParent(TargetHandle, DesktopHandle);

        }

        private void UnborderlessButton_Click(object sender, RoutedEventArgs e)
        {
            // Yes I am well-aware this line is insanely long and could be shorter, and that catching everything is an awful idea. It is never going to be updated though because I know it annoys Sylly and that's cute.
            try
            {
                AddBorders(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]), RetrieveWindowProperties(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]), 0), RetrieveWindowProperties(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]), 1));
            }
            catch (Exception) { }
        }

        public void RefreshButton_Click(object sender, RoutedEventArgs e)
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
                    StoreWindowProperties(ChildHandle.ToInt32(), (uint)GetWindowLong(ChildHandle.ToInt32(), WeirdMagicalNumbers.GWL_STYLE), (uint)GetWindowLong(ChildHandle.ToInt32(), WeirdMagicalNumbers.GWL_EXSTYLE));
                    HandleListBox.Items.Add(WindowList[count][1] + " " + WindowList[count][0]);
                    count++;

                }
            }
        }

        private void SaveList_Clicked(object sender, RoutedEventArgs e)
        {
            // File.WriteAllText(@"testADP.csv", string.Join("§", CSVArray));
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            
        }



        private void BorderlessButton_Click(object sender, RoutedEventArgs e)
        { // This long potato does some mighty magic that takes the ID and gets the handle from the WindowList thing that I made above ^
            if (HandleListBox.SelectedItem != null)
            {
                RemoveBorders(Convert.ToInt32(WindowList[Convert.ToInt32(HandleListBox.SelectedItem.ToString().Substring(HandleListBox.SelectedItem.ToString().Length - 3))][2]));
            }
        }

        private void AppList_Clicked(object sender, MouseButtonEventArgs e)
        {
            RefreshButton_Click(null, null);
        }


        // ///////////////////////////////////////////////////////////// //
        // All the weird non-GUIey bits are here don't question it shhhh //
        // ///////////////////////////////////////////////////////////// //


        // Refresh the Saved Applications List with data from the array
        private void UpdateSAL()
        {
            foreach (object name in CSVArray)
            {
                SavedListBox.Items.Add((string)name);
            }
        }

        // Bits and bobs for finding and adjusting windows
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
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr Handle, Rect WinRect);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowLong(int hWnd, int nIndex);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowLong(int hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

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

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

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

        public void RemoveBorders(int TargetHandle)
        {
            uint nStyle = (uint)GetWindowLong(TargetHandle, WeirdMagicalNumbers.GWL_STYLE);
            nStyle = (nStyle | (WeirdMagicalNumbers.WS_THICKFRAME + WeirdMagicalNumbers.WS_DLGFRAME + WeirdMagicalNumbers.WS_BORDER)) ^ (WeirdMagicalNumbers.WS_THICKFRAME + WeirdMagicalNumbers.WS_DLGFRAME + WeirdMagicalNumbers.WS_BORDER);
            SetWindowLong(TargetHandle, WeirdMagicalNumbers.GWL_STYLE, nStyle);

            nStyle = (uint)GetWindowLong(TargetHandle, WeirdMagicalNumbers.GWL_EXSTYLE);
            nStyle = (nStyle | (WeirdMagicalNumbers.WS_EX_DLGMODALFRAME + WeirdMagicalNumbers.WS_EX_WINDOWEDGE + WeirdMagicalNumbers.WS_EX_CLIENTEDGE + WeirdMagicalNumbers.WS_EX_STATICEDGE)) ^ (WeirdMagicalNumbers.WS_EX_DLGMODALFRAME + WeirdMagicalNumbers.WS_EX_WINDOWEDGE + WeirdMagicalNumbers.WS_EX_CLIENTEDGE + WeirdMagicalNumbers.WS_EX_STATICEDGE);
            SetWindowLong(TargetHandle, WeirdMagicalNumbers.GWL_EXSTYLE, nStyle);

            uint uFlags = WeirdMagicalNumbers.SWP_NOSIZE | WeirdMagicalNumbers.SWP_NOMOVE | WeirdMagicalNumbers.SWP_NOZORDER | WeirdMagicalNumbers.SWP_NOACTIVATE | WeirdMagicalNumbers.SWP_NOOWNERZORDER | WeirdMagicalNumbers.SWP_NOSENDCHANGING | WeirdMagicalNumbers.SWP_FRAMECHANGED;
            SetWindowPos(TargetHandle, 0, 0, 0, 0, 0, uFlags);
        }

        public void AddBorders(int SelectedHandle, uint nStyle, uint nStyleEx)
        {
            SetWindowLong(SelectedHandle, WeirdMagicalNumbers.GWL_STYLE, nStyle);

            SetWindowLong(SelectedHandle, WeirdMagicalNumbers.GWL_EXSTYLE, nStyleEx);

            uint uFlags = WeirdMagicalNumbers.SWP_NOSIZE | WeirdMagicalNumbers.SWP_NOMOVE | WeirdMagicalNumbers.SWP_NOZORDER | WeirdMagicalNumbers.SWP_NOACTIVATE | WeirdMagicalNumbers.SWP_NOOWNERZORDER | WeirdMagicalNumbers.SWP_NOSENDCHANGING | WeirdMagicalNumbers.SWP_FRAMECHANGED;
            SetWindowPos(SelectedHandle, 0, 0, 0, 0, 0, uFlags);
        }

        static class WeirdMagicalNumbers
        {
            // Important magical numbers that make windows borderless
            public const int GWL_STYLE = -16;
            public const int GWL_EXSTYLE = -20;
            public const uint SWP_NOSIZE = 0x01;
            public const uint SWP_NOMOVE = 0x02;
            public const uint SWP_NOZORDER = 0x04;
            public const uint SWP_NOACTIVATE = 0x10;
            public const uint SWP_NOOWNERZORDER = 0x200;
            public const uint SWP_NOSENDCHANGING = 0x400;
            public const uint SWP_FRAMECHANGED = 0x20;
            public const uint WS_THICKFRAME = 0x40000;
            public const uint WS_DLGFRAME = 0x400000;
            public const uint WS_BORDER = 0x800000;
            public const uint WS_EX_DLGMODALFRAME = 1;
            public const uint WS_EX_WINDOWEDGE = 0x100;
            public const uint WS_EX_CLIENTEDGE = 0200;
            public const uint WS_EX_STATICEDGE = 0x20000;
            public const int SW_SHOWNOACTIVATE = 4;
            public const int SW_RESTORE = 9;
            public const int WM_EXITSIZEMOVE = 0x0232;

        }


        // Checks for AppData directory/CSV and creates it if it doesnt exist
        private void FileSystem()
        {
            string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            LocalFolder = System.IO.Path.Combine(AppData, "ActiveDesktopPlus");

            Directory.CreateDirectory(LocalFolder);

            if (!File.Exists(System.IO.Path.Combine(LocalFolder, "saved.csv")))
            {
                _ = File.Create(System.IO.Path.Combine(LocalFolder, "saved.csv"));
            }
        }


        // Reads data from the CSV, stolen from my other project VRMID
        public string[][] ReadCSV()
        {
            int lineCount = File.ReadAllLines(System.IO.Path.Combine(LocalFolder, "saved.csv")).Length;
            string[][] daCSVData = new string[lineCount][];
            int n = 0;
            string lineToBeProcessed;
            string[] values;
            do
            {
                lineToBeProcessed = File.ReadLines(System.IO.Path.Combine(LocalFolder, "saved.csv")).Skip(n).Take(1).First();
                lineToBeProcessed = lineToBeProcessed.Replace(" ", string.Empty);
                values = lineToBeProcessed.Split('§');
                daCSVData[n] = values;
                n++;
            }
            while (n <= lineCount - 1);
            return daCSVData;
        }

    }
}
