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
    
    //public enum DesktopWindow
    //{
    //    ProgMan,
    //    SHELLDLL_DefViewParent,
    //    SHELLDLL_DefView,
    //    SysListView32
    //}
    


    public partial class MainWindow : Window
    {

        IntPtr DesktopHandle;
        IntPtr TargetHandle;

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


    }
}
