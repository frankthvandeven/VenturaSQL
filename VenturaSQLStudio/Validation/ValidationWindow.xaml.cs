using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace VenturaSQLStudio.Validation
{
    /// <summary>
    /// Interaction logic for ValidationWindow.xaml
    /// </summary>
    public partial class ValidationWindow : VenturaWindow
    {
        private ValidationEngine _engine;

        private const int GWL_STYLE = -16,
                     WS_MAXIMIZEBOX = 0x10000,
                     WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

        public ValidationWindow(ValidationEngine engine)
        {
            _engine = engine;

            InitializeComponent();

            listView.ItemsSource = engine.ValidationMessages;

            this.ResizeMode = ResizeMode.CanResizeWithGrip;

            this.SourceInitialized += (x, y) =>
            {
                /* Hide the minimize and maximize buttons */
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                var currentStyle = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
            };

        }

        private void listView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gridView = listView.View as GridView;

            var actualWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;

            for (Int32 i = 0; i < (gridView.Columns.Count-1); i++)
            {
                actualWidth = actualWidth - gridView.Columns[i].ActualWidth;
            }

            if (actualWidth < 200)
                actualWidth = 200;

            gridView.Columns[gridView.Columns.Count-1].Width = actualWidth;
        }


    }
}
