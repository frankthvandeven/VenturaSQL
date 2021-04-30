using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VenturaSQLStudio {
    /// <summary>
    /// Interaction logic for GettingStartedControl.xaml
    /// </summary>
    [ContentProperty("Inlines")]
    public partial class GettingStartedControl : UserControl
    {
        public GettingStartedControl()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public InlineCollection Inlines
        {
            get { return textblock.Inlines; }
        }

        static GettingStartedControl()
        {
            GettingStartedControl.SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(GettingStartedControl), new PropertyMetadata(null));
        }

        public readonly static DependencyProperty SourceProperty;

        public string FileName
        {
            set { brush.ImageSource = GetImageFromFilename(value); }
        }

        private ImageSource GetImageFromFilename(string filename)
        {
            string uriString = $@"pack://application:,,,/VenturaSQLStudio;component/StartPage/Images/{filename}";

            Uri u = new Uri(uriString, UriKind.RelativeOrAbsolute);

            BitmapImage bmi = new BitmapImage(u);
            
            // prevents error 'Must create DependencySource on same Thread as the DependencyObject'
            if (bmi.CanFreeze == true)
                bmi.Freeze();

            return bmi;
        }

    }
}
