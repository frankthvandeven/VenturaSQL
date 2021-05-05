using SysdevDTO;
using System;
using System.Threading.Tasks;
using VenturaSQLStudio.Helpers;

namespace VenturaSQLStudio
{
    public delegate void CheckForUpdateEventHandler(Version latest_version);

    public class CheckForUpdate
    {
        public event CheckForUpdateEventHandler CheckForUpdateEvent;

        public async Task RunAsync()
        {
            var url = "https://dotnet.sysdev.nl/api/checkforupdate";

#if DEBUG
            //url = "https://localhost:44345/api/checkforupdate";
#endif

            string product_key = "";

#if LICENSE_MANAGER
            if (Central.RefreshResult == RefreshResult.ValidLicenseKey)
                product_key = Central.License_ProductKey;
#endif
 
            var requestData = new CheckForUpdateDTO();

            requestData.Product = "venturasql-studio";
            requestData.Version = MainWindow.ViewModel.VenturaVersion.ToString(3);
            requestData.ProductKey = product_key;
            requestData.MachineHash = StudioGeneral.GetMachineHash();

            UpdateCheckResultDTO result = await StudioHttp.PostJsonAsync<UpdateCheckResultDTO>(url, requestData);

            Version v = new Version(result.LatestVersion);

            CheckForUpdateEvent(v);
        }

    } // class
} // namespace
