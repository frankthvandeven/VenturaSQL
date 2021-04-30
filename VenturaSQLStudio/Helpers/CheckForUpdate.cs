using System;
using System.ComponentModel;
using VenturaSQLStudio.Helpers;

namespace VenturaSQLStudio
{

    public delegate void CheckForUpdateEventHandler(Version latest_version);

    public class CheckForUpdate
    {
        public event CheckForUpdateEventHandler CheckForUpdateEvent;

        public void RunAsync()
        {
            // Check for update in background
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += Worker_DoWork;
            //worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WebServiceCaller wsc = new WebServiceCaller("WebApplication.WebServices.CheckForUpdateHandler");

            // Collect information...
            string ventura_version = MainWindow.ViewModel.VenturaVersion.ToString(3);

            string product_key = "";

#if LICENSE_MANAGER
            if (Central.RefreshResult == RefreshResult.ValidLicenseKey)
                product_key = Central.License_ProductKey;
#endif
            string machine_hash = StudioGeneral.GetMachineHash();

            // Binarize it...
            wsc.Request_bw.Write((byte)2); // Indicates data format generation
            wsc.Request_bw.Write("VenturaStudio");
            wsc.Request_bw.Write(ventura_version);
            wsc.Request_bw.Write(product_key);
            wsc.Request_bw.Write(machine_hash);

            // Execute the request...
            wsc.ExecRequest();

            // Read the response...
            string server_reported_version = wsc.Response_br.ReadString();

            Version v = new Version(server_reported_version);

            CheckForUpdateEvent(v);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

    } // class
} // namespace
