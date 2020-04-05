using System;
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

        IntPtr DesktopHandle;
        IntPtr TargetHandle;
        List<List<string>> WindowList;

        public MainWindow()
        {
            InitializeComponent();

            IntPtr RootHandle = FindWindowExA(IntPtr.Zero, IntPtr.Zero, "WorkerW", "");
            DesktopHandle = FindWindowExA(RootHandle, IntPtr.Zero, "SHELLDLL_DefView", "");

            while (DesktopHandle == IntPtr.Zero)
            {
                RootHandle = FindWindowExA(IntPtr.Zero, RootHandle, "WorkerW", "");
                DesktopHandle = FindWindowExA(RootHandle, IntPtr.Zero, "SHELLDLL_DefView", "");
            }
            RefreshButton_Click(null, null);
        }


        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.B)
            {
                TargetHandle = WindowFromPoint(Convert.ToInt32(GetCursorPosition().X), Convert.ToInt32(GetCursorPosition().Y));
                HwndInputTextBox.Text = TargetHandle.ToString();
            }
        }

        private void ApplyHwndButton_Click(object sender, RoutedEventArgs e)
        {
            SetParent(TargetHandle, DesktopHandle);
        }

        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

            StringBuilder lpString = new StringBuilder(100);
            int count = 0;
            WindowList = new List<List<string>>();

            // Checks every child of the desktop to see if they're a direct root window, then if they are, assigns them a numerical ID and adds their title and handle to a list.
            HandleListBox.Items.Clear();
            for (IntPtr ChildHandle = FindWindowExA(DesktopHandle, IntPtr.Zero, null, null); ChildHandle != IntPtr.Zero; ChildHandle = FindWindowExA(DesktopHandle, ChildHandle, null, null))
            {
                int result = GetWindowText(ChildHandle, lpString, 100);
                if (result != 101)
                {
                    WindowList.Add(new List<string>()); 
                    WindowList[count].Add((1000 + count).ToString()); // ID - Nobody is ever going to have more than 1000 windows open, if they do this will break but hey idc
                    WindowList[count].Add(lpString.ToString()); // Window Title
                    WindowList[count].Add(ChildHandle.ToString()); // Window Handle
                    WindowList[count].Add("2");  // Is borderless - defaulting to unknown

                    HandleListBox.Items.Add(WindowList[count][1] + " " + WindowList[count][0]);
                    count++;

                }
            }
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

        // Bits and bobs for finding and adjusting windows
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowExA(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(int xPoint, int yPoint);
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndParent);
        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
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

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }


        // This bit gets the titles of all the active windows. It's probably massively overcomplicated but it works and that's all I care about right now.
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, IntPtr i);

    
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
            uint nStyle = (uint)GetWindowLong(TargetHandle, ValuesForStuff.GWL_STYLE);
            nStyle = (nStyle | (ValuesForStuff.WS_THICKFRAME + ValuesForStuff.WS_DLGFRAME + ValuesForStuff.WS_BORDER)) ^ (ValuesForStuff.WS_THICKFRAME + ValuesForStuff.WS_DLGFRAME + ValuesForStuff.WS_BORDER);
            SetWindowLong(TargetHandle, ValuesForStuff.GWL_STYLE, nStyle);

            nStyle = (uint)GetWindowLong(TargetHandle, ValuesForStuff.GWL_EXSTYLE);
            nStyle = (nStyle | (ValuesForStuff.WS_EX_DLGMODALFRAME + ValuesForStuff.WS_EX_WINDOWEDGE + ValuesForStuff.WS_EX_CLIENTEDGE + ValuesForStuff.WS_EX_STATICEDGE)) ^ (ValuesForStuff.WS_EX_DLGMODALFRAME + ValuesForStuff.WS_EX_WINDOWEDGE + ValuesForStuff.WS_EX_CLIENTEDGE + ValuesForStuff.WS_EX_STATICEDGE);
            SetWindowLong(TargetHandle, ValuesForStuff.GWL_EXSTYLE, nStyle);

            uint uFlags = ValuesForStuff.SWP_NOSIZE | ValuesForStuff.SWP_NOMOVE | ValuesForStuff.SWP_NOZORDER | ValuesForStuff.SWP_NOACTIVATE | ValuesForStuff.SWP_NOOWNERZORDER | ValuesForStuff.SWP_NOSENDCHANGING | ValuesForStuff.SWP_FRAMECHANGED;
            SetWindowPos(TargetHandle, 0, 0, 0, 0, 0, uFlags);
        }

        static class ValuesForStuff
        {
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
    }
}
