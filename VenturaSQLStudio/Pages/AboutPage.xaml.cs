using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VenturaSQLStudio.Pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage : UserControl
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            StudioGeneral.StartBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        //private void btnClose_Click(object sender, RoutedEventArgs e)
        //{
        //    MainWindow window = (MainWindow)Application.Current.MainWindow;
        //    window.CloseTabContainingPage(this);
        //}
    }
}
