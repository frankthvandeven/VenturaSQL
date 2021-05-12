using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using VenturaSQLStudio.ProviderHelpers;

namespace VenturaSQLStudio.Pages
{
    /// <summary>
    /// Interaction logic for ProviderPage.xaml
    /// </summary>
    public partial class ProviderPage : UserControl
    {
        
        public ProviderPage()
        {
            InitializeComponent();
            
            lvProviders.ItemsSource = MainWindow.ViewModel.ProviderRepository;
            
            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvProviders.ItemsSource);
            //if (view.GroupDescriptions.Count == 0)
            //{
            //    PropertyGroupDescription groupDescription = new PropertyGroupDescription("IsInstalled", new GroupNameConverter());
            //    view.GroupDescriptions.Add(groupDescription);
            //}

            int vsql_version = Assembly.GetExecutingAssembly().GetName().Version.Major;

            textblockInfo.Text = $"VenturaSQL Studio {vsql_version} runs on .NET {Environment.Version.Major}, and this runtime does not support the dynamic " +
                                 "loading of ADO.NET providers. Provider DLLs must be linked into the VenturaSQLStudio executable. " +
                                 "Contact fvv@sysdev.nl if you need a provider added.";

        }

        /// <summary>
        /// Select another provider
        /// </summary>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            Project project = MainWindow.ViewModel.CurrentProject;

            ProviderHelper ph = lvProviders.SelectedItem as ProviderHelper;

            if (ph == null)
                return;

            DynamicallySwitchProvider dsp = new DynamicallySwitchProvider();

            bool result = dsp.Exec(ph.ProviderInvariantName);

            if (result == false)
                return;

            // Open and select the project settings page:
            MainWindow window = (MainWindow)Application.Current.MainWindow;

            window.CloseTabContainingPage(this);

            window.DoOpenProjectSettings();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            StudioGeneral.StartBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

    }

    //public class GroupNameConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        bool is_installed = (bool)value;
    //        if (is_installed)
    //            return "Available";
    //        return "Optional providers";
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
