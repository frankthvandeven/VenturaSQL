using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VenturaSQLStudio.Pages
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : UserControl
    {
        MainViewModel _mainmodel;

        public OptionsPage(MainViewModel model)
        {
            InitializeComponent();

            _mainmodel = model;

            this.DataContext = model;
        }

        private void buttonClearMRU_Click(object sender, RoutedEventArgs e)
        {
            string message = "The MRU list on the start page will be cleared. Continue?";

            MessageBoxResult result = MessageBox.Show(message, "VenturaSQL Studio", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);

            if (result != MessageBoxResult.Yes)
                return;

            MainWindow.ViewModel.MRU.Clear();
        }

    }
}
