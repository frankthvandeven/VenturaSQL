using System;
using System.Data.Common;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VenturaSQLStudio {
    public class ProviderInfo : ViewModelBase
    {
        private string _provider_invariant_name;
        private string _name;
        private string _description;
        private string _company;
        private string _link;
        private ImageSource _product_image;

        private DbProviderFactory _factory;

        public ProviderInfo(string provider_invariant_name)
        {
            _provider_invariant_name = provider_invariant_name;

            _company = null;
            _product_image = null;
            _link = "none";

        }

        public string ProviderInvariantName
        {
            get { return _provider_invariant_name; }
        }

        public DbProviderFactory Factory
        {
            get { return _factory; }
            set
            {
                if (_factory == value)
                    return;

                _factory = value;

                NotifyPropertyChanged("Factory");
            }
        }

        public string FactoryAsString { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                    return;

                _name = value;

                NotifyPropertyChanged("Name");
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value)
                    return;

                _description = value;

                NotifyPropertyChanged("Description");
            }
        }

        public string Company
        {
            get { return _company; }
            set
            {
                if (_company == value)
                    return;

                _company = value;

                NotifyPropertyChanged("Company");
                NotifyPropertyChanged("CompanyVisible");
            }
        }

        public Visibility CompanyVisible
        {
            get
            {
                if (_company == null)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public string Link
        {
            get { return _link; }
            set
            {
                if (_link == value)
                    return;

                if (value == null)
                    throw new ArgumentNullException("Link");

                _link = value;

                NotifyPropertyChanged("Link");
                NotifyPropertyChanged("LinkVisible");
            }
        }

        public Visibility LinkVisible
        {
            get
            {
                if (_link == "none")
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public ImageSource ProductImage
        {
            get
            {
                if (_product_image != null)
                    return _product_image;

                // The default icons come from:
                // https://www.flaticon.com/search?word=data

                if ( this.IsInstalled)
                    return GetProductImageFromFilename("default_installed.png");

                return GetProductImageFromFilename("default_not_installed.png");

            }
            set
            {
                if (_product_image == value)
                    return;

                _product_image = value;

                NotifyPropertyChanged("ProductImage");
            }


        }

        public Brush BorderBrush
        {
            get
            {
                if (MainWindow.ViewModel.CurrentProject.ProviderInvariantName == _provider_invariant_name)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0173C7"));
                else
                    return Brushes.Transparent;
            }
        }

        public bool IsInstalled
        {
            get
            {
                return _factory != null;
            }
        }

        public bool IsSelectedProvider
        {
            get
            {
                if (MainWindow.ViewModel.CurrentProject.ProviderInvariantName == _provider_invariant_name)
                    return true;
                else
                    return false;
            }
        }

        #region Static method

        /// <summary>
        /// Filename like 'default_installed.png'.
        /// </summary>
        public static ImageSource GetProductImageFromFilename(string filename)
        {
            //Uri u = new Uri($"/Pages/ProviderPage/ProductImages/{filename}", UriKind.Relative);

            string uriString = $@"pack://application:,,,/VenturaSQLStudio;component/Pages/ProviderPage/ProductImages/{filename}";

            Uri u = new Uri(uriString, UriKind.RelativeOrAbsolute);

            BitmapImage bmi = new BitmapImage(u);

            // prevents error 'Must create DependencySource on same Thread as the DependencyObject'
            if (bmi.CanFreeze == true)
                bmi.Freeze();

            return bmi;
        }

        #endregion

    }

}
