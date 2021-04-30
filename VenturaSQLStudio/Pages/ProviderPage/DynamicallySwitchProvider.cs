using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VenturaSQL;

namespace VenturaSQLStudio.Pages
{
    internal class DynamicallySwitchProvider
    {
        private Project _project;

        internal DynamicallySwitchProvider()
        {
            _project = MainWindow.ViewModel.CurrentProject;
        }

        internal bool Exec(string provider_invariant_name)
        {
            if (_project.ProviderInvariantName == provider_invariant_name)
                return false;

            bool advanced_settings_are_default = _project.AdvancedSettings.IsSetToDefaultValues();

            if (advanced_settings_are_default == false)
            {
                string message = "The Avanced Provider Settings in the Project Settings page have been modified. " +
                    "Selecting a different provider will reset the advanced settings to default values.\n\n\n" +
                    "Do you want to continue selecting the provider?";

                MessageBoxResult result = MessageBox.Show(message, "VenturaSQL Studio", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.OK);

                if (result != MessageBoxResult.OK)
                    return false;
            }

            AdoConnector connector = null;

            try
            {
                connector = AdoConnectorHelper.Create(provider_invariant_name);
            }
            catch (Exception ex)
            {
                string message = $"Loading assembly for provider \"{provider_invariant_name}\" failed.\n\n" +
                    "The provider could not be selected.\n\n\n" +
                    ex.Message;

                MessageBox.Show(message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // The provider was selected!
            _project.ProviderInvariantName = provider_invariant_name;

            // Reset Advanced Provider Settings
            _project.AdvancedSettings.ResetToDefault();

            // HERE WE COULD BE LOADING PRE-SET ADVANCED DEFAULT SETTINGS.

            // Update the loaded Recordsets
            _project.ProviderRelatedSettingsWereModified();

            return true;
        }

    }
}
