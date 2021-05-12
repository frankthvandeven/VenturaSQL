using System;
using System.Data.Common;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VenturaSQLStudio.ProviderHelpers
{
    public abstract class ProviderHelper
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public string Company { get; protected set; }
        public string Link { get; protected set; }
        public string FactoryAsString { get; protected set; }


        public DbProviderFactory Factory { get; protected set; }

        private ImageSource _product_image;
        private string _provider_invariant_name;

        public ProviderHelper()
        {
            Type t = this.GetType();
            object[] attributes = t.GetCustomAttributes(false);

            if (attributes.Length != 1)
                throw new Exception($"Missing ProviderInvariantName attribute on {t.FullName}");

            ProviderInvariantNameAttribute attrib = attributes[0] as ProviderInvariantNameAttribute;

            if (attrib == null)
                throw new Exception($"ProviderInvariantName should be the only attribute on {t.FullName}");

            _provider_invariant_name = attrib.ProviderInvariantName;

        }

        public string ProviderInvariantName
        {
            get { return _provider_invariant_name; }
        }

        public Visibility CompanyVisible
        {
            get
            {
                if (Company == null)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public Visibility LinkVisible
        {
            get
            {
                if (Link == "none")
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

                return GetProductImageFromFilename("default_installed.png");

            }
            protected set { _product_image = value; }

        }

        public Brush BorderBrush
        {
            get
            {
                if (MainWindow.ViewModel.CurrentProject.ProviderInvariantName == this.ProviderInvariantName)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0173C7"));
                else
                    return Brushes.Transparent;
            }
        }

        /// <summary>
        /// This is to support the ListView.
        /// </summary>
        public bool IsSelectedProvider
        {
            get
            {
                if (MainWindow.ViewModel.CurrentProject.ProviderInvariantName == this.ProviderInvariantName)
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
