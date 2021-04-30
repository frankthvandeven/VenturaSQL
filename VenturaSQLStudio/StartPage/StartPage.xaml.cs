using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VenturaSQLStudio.Helpers;
using VenturaSQLStudio.Pages;

namespace VenturaSQLStudio {
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        // Nice backgrounds for filepage:
        // http://alssndro.github.io/trianglify-background-generator/

        MainWindow _mainwindow;

        public ViewModelClass ViewModel { get; } = new ViewModelClass();

        public StartPage()
        {
            InitializeComponent();

            _mainwindow = (MainWindow)Application.Current.MainWindow;
            _itemscontrolMRU.ItemsSource = MainWindow.ViewModel.MRU.Collection;

            // Monitor open/close project and project filename.
            MainWindow.ViewModel.PropertyChanged += MainViewModel_PropertyChanged;

            ResetProjectInfo();
        }

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CurrentProject" && e.PropertyName != "FileName")
                return;

            Action action = () =>
            {
                ResetProjectInfo();
            };

            Dispatcher.Invoke(action);

        }

        private void ResetProjectInfo()
        {
            // Update project filename and project folder display.
            ViewModel.NotifyPropertyChanged(null);

            if (MainWindow.ViewModel.CurrentProject == null)
            {
                sectionProjectOpen.Visibility = Visibility.Collapsed;
                sectionProjectClosed.Visibility = Visibility.Visible;

                blockNoOpenProjectInfo.Visibility = Visibility.Visible;
                blockOpenProjectInfo.Visibility = Visibility.Collapsed;
            }
            else
            {
                sectionProjectOpen.Visibility = Visibility.Visible;
                sectionProjectClosed.Visibility = Visibility.Collapsed;

                blockNoOpenProjectInfo.Visibility = Visibility.Collapsed;
                blockOpenProjectInfo.Visibility = Visibility.Visible;
            }

        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            _mainwindow.DoNew();
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            _mainwindow.DoOpen();
        }

        private void ButtonCloseProject_Click(object sender, RoutedEventArgs e)
        {
            _mainwindow.DoClose();
        }

        private void MenuItemOpenContainingFolder_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu_item = (MenuItem)sender as MenuItem;
            MostRecentlyUsedListItem mru_item = (MostRecentlyUsedListItem)menu_item.DataContext;

            OpenFileExplorerWindow.Exec(mru_item.FullFilePath);
        }

        private void MenuItemRemoveFromList_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu_item = (MenuItem)sender as MenuItem;
            MostRecentlyUsedListItem mru_item = (MostRecentlyUsedListItem)menu_item.DataContext;

            MainWindow.ViewModel.MRU.Remove(mru_item);
        }

        private void Button_OpenProjectFromMRU_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender as Button;
            MostRecentlyUsedListItem mru_item = (MostRecentlyUsedListItem)button.DataContext;

            _mainwindow.DoOpen(mru_item.FullFilePath);
        }

        private void MenuItem_OpenProjectFromMRU_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu_item = (MenuItem)sender as MenuItem;
            MostRecentlyUsedListItem mru_item = (MostRecentlyUsedListItem)menu_item.DataContext;

            _mainwindow.DoOpen(mru_item.FullFilePath);
        }


        public class ViewModelClass : ViewModelBase
        {
            public string ProjectFilename
            {
                get
                {
                    if (MainWindow.ViewModel.FileName == null)
                        return "(none)";

                    return Path.GetFileNameWithoutExtension(MainWindow.ViewModel.FileName);
                }
            }

            public string ProjectFolder
            {
                get
                {
                    if (MainWindow.ViewModel.FileName == null)
                        return "(none)";

                    return Path.GetDirectoryName(MainWindow.ViewModel.FileName);
                }
            }


        } // end of viewmodel

        private void Hyperlink_ProjectSettings(object sender, RoutedEventArgs e)
        {
            _mainwindow.DoOpenProjectSettings();
        }

        private void Hyperlink_NewProject(object sender, RoutedEventArgs e)
        {
            _mainwindow.DoNew();
        }

        private void Hyperlink_AutoCreate(object sender, RoutedEventArgs e)
        {
            AutoCreateSettingsWindow window = new AutoCreateSettingsWindow(MainWindow.ViewModel.CurrentProject);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
    }
}
