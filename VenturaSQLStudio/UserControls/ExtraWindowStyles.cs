using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

// Documentation:
// http://codeflow49.blogspot.com/2010/08/resizable-wpf-windows-without-minimize.html

namespace VenturaSQLStudio {
    class ExtraWindowStyles : DependencyObject
    {
        #region Windows API Imports

        const int GWL_STYLE = (-16);

        [Flags]
        private enum WindowStyles : uint
        {
            WS_SYSMENU = 0x80000,
            WS_MINIMIZEBOX = 0x20000,
            WS_MAXIMIZEBOX = 0x10000,
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        public const int SWP_FRAMECHANGED = 0x0020;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOOWNERZORDER = 0x0200;
        public const int SWP_NOREPOSITION = 0x0200;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOZORDER = 0x0004;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        #endregion


        #region Helpers

        private static void AddWindowStyle(Window window, WindowStyles styleToAdd)
        {
            WindowInteropHelper wih = new WindowInteropHelper(window);
            WindowStyles style = (WindowStyles)GetWindowLongPtr(wih.EnsureHandle(), GWL_STYLE);
            style |= styleToAdd;
            SetWindowLongPtr(wih.Handle, GWL_STYLE, (IntPtr)style);
            SetWindowPos(wih.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOREPOSITION | SWP_NOSIZE | SWP_NOZORDER);
        }

        private static void RemoveWindowStyle(Window window, WindowStyles styleToRemove)
        {
            WindowInteropHelper wih = new WindowInteropHelper(window);
            WindowStyles style = (WindowStyles)GetWindowLongPtr(wih.EnsureHandle(), GWL_STYLE);
            style &= ~styleToRemove;
            SetWindowLongPtr(wih.Handle, GWL_STYLE, (IntPtr)style);
            SetWindowPos(wih.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOREPOSITION | SWP_NOSIZE | SWP_NOZORDER);
        }

        #endregion


        #region CanMinimize attached property

        public static bool GetCanMinimize(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanMinimizeProperty);
        }

        public static void SetCanMinimize(DependencyObject obj, bool value)
        {
            obj.SetValue(CanMinimizeProperty, value);
        }

        public static readonly DependencyProperty CanMinimizeProperty =
            DependencyProperty.RegisterAttached("CanMinimize", typeof(bool), typeof(ExtraWindowStyles), new UIPropertyMetadata(true, OnCanMinimizeChanged));

        private static void OnCanMinimizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window == null)
                return;

            if ((bool)e.NewValue)
                AddWindowStyle(window, WindowStyles.WS_MINIMIZEBOX);
            else
                RemoveWindowStyle(window, WindowStyles.WS_MINIMIZEBOX);
        }

        #endregion

        #region CanMaximize attached property

        public static bool GetCanMaximize(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanMaximizeProperty);
        }

        public static void SetCanMaximize(DependencyObject obj, bool value)
        {
            obj.SetValue(CanMaximizeProperty, value);
        }

        public static readonly DependencyProperty CanMaximizeProperty =
            DependencyProperty.RegisterAttached("CanMaximize", typeof(bool), typeof(ExtraWindowStyles), new UIPropertyMetadata(true, OnCanMaximizeChanged));

        private static void OnCanMaximizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window == null)
                return;

            if ((bool)e.NewValue)
                AddWindowStyle(window, WindowStyles.WS_MAXIMIZEBOX);
            else
                RemoveWindowStyle(window, WindowStyles.WS_MAXIMIZEBOX);
        }

        #endregion

        #region HasSystemMenu attached property

        public static bool GetHasSystemMenu(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasSystemMenuProperty);
        }

        public static void SetHasSystemMenu(DependencyObject obj, bool value)
        {
            obj.SetValue(HasSystemMenuProperty, value);
        }

        public static readonly DependencyProperty HasSystemMenuProperty =
            DependencyProperty.RegisterAttached("HasSystemMenu", typeof(bool), typeof(ExtraWindowStyles), new UIPropertyMetadata(true, OnHasSystemMenuChanged));

        private static void OnHasSystemMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window == null)
                return;

            if ((bool)e.NewValue)
                AddWindowStyle(window, WindowStyles.WS_SYSMENU);
            else
                RemoveWindowStyle(window, WindowStyles.WS_SYSMENU);
        }

        #endregion
    }
}
