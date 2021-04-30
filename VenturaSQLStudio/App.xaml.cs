using System;
using System.Data.Common;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VenturaSQLStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string StartUpProject = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            // When a control is disabled, still display the tooltip.
            ToolTipService.ShowOnDisabledProperty.OverrideMetadata(
            typeof(Control),
            new FrameworkPropertyMetadata(true));

            // Select the text in a TextBox when it receives focus.
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotKeyboardFocusEvent,
                new RoutedEventHandler(SelectAllText));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.MouseDoubleClickEvent,
                new RoutedEventHandler(SelectAllText));

            //PresentationTraceSources.Refresh();
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
            //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1 && e.Args[0] != "/unregister")
            {
                App.StartUpProject = e.Args[0];
                return;
            }

#if LICENSE_MANAGER
            if (e.Args.Length == 1 && e.Args[0] == "/unregister")
            {
                int exitcode = 0;

                string license_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "VenturaSQLStudio");
                string license_file = Path.Combine(license_folder, "venturasql.lic");

                try
                {
                    if (File.Exists(license_file))
                        File.Delete(license_file);
                }
                catch (Exception ex)
                {
                    exitcode = 1;
                    MessageBox.Show($"Error deleting license file {license_file}\n\n" + ex.Message, "Delete file error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                this.Shutdown(exitcode);

            }

            //Display parameters, for debugging  
            //string[] args = e.Args;
            //for (int i = 0; i < args.Length; i++)
            //{
            //    string arg = args[i];
            //    MessageBox.Show($"Arg {i} value [{arg}]");
            //}

            if (e.Args.Length == 2 && e.Args[0] == "/register")
            {
                int exitcode = 0;

                string from_file = e.Args[1];
                string to_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "VenturaSQLStudio");
                string to_file = Path.Combine(to_folder, "venturasql.lic");

                try
                {
                    Directory.CreateDirectory(to_folder);
                    File.Copy(from_file, to_file, true);
                }
                catch (Exception ex)
                {
                    exitcode = 1;
                    MessageBox.Show($"Error copying file.\n\nSource: {from_file}\n\nTarget: {to_file}\n\n" + ex.Message, "Copy error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                this.Shutdown(exitcode);
            }
#endif

        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string message = "There was a problem." +
                             Environment.NewLine + Environment.NewLine +
                            e.Exception.Message;

            MessageBoxResult result = MessageBox.Show(message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.Yes);

            if (result == MessageBoxResult.OK)
                e.Handled = true;
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focused, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }

    }
}

