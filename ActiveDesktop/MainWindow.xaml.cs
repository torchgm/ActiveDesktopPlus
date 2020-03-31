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
        IntPtr ActiveHandle = IntPtr.Zero;
        List<string> ActiveHandleStrings = new List<string>();
        List<IntPtr> ActiveHandles = new List<IntPtr>();

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
            //ActiveHandles.Add(TargetHandle.ToString()); // Currently storing active handles as strings, will probably convert back to IntPtrs eventually
            //HandleListBox.Items.Add(TargetHandle.ToString());
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder lpString = new StringBuilder(100);
            HandleListBox.Items.Clear();
            ActiveHandles = GetChildWindows(DesktopHandle);
            for (int i = 0; i < ActiveHandles.Count; i++)
            {
                int result = GetWindowText(ActiveHandles[i], lpString, 100);
                if (result != 101)
                {
                    HandleListBox.Items.Add(lpString.ToString());
                }
            }
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
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

    }
}
