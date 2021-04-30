using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Helpers
{
    public class WebServiceCaller
    {
        private MemoryStream _request_ms;
        private BinaryWriter _request_bw;

        private MemoryStream _response_ms;
        private BinaryReader _response_br;

        private string _url_string;

        public WebServiceCaller(string full_classname)
        {
            _url_string = "https://api.sysdev.nl/Ventura.WBSRVC";

            //#if DEBUG
            //           _url_string = "https://localhost:44345/Ventura.WBSRVC";
            //#endif

            _request_ms = new MemoryStream();
            _request_bw = new BinaryWriter(_request_ms);

            _response_ms = null;
            _response_br = null;

            _request_bw.Write((short)1); // webservice generation
            _request_bw.Write(full_classname); // handler
        }

        public void ExecRequest()
        {
            byte[] result_array = BinaryHttpRequest.Request(_url_string, _request_ms);

            _response_ms = new MemoryStream(result_array);
            _response_br = new BinaryReader(_response_ms);

            string error_message = _response_br.ReadString();

            if (error_message.Length > 0)
                throw new Exception($"Server reported error:\n{error_message}");
        }

        public async Task ExecRequestAsync()
        {
            byte[] result_array = await BinaryHttpRequest.RequestAsync(_url_string, _request_ms);

            _response_ms = new MemoryStream(result_array);
            _response_br = new BinaryReader(_response_ms);

            string error_message = _response_br.ReadString();

            if (error_message.Length > 0)
                throw new Exception($"Error on Remote: {error_message}");
        }

        public MemoryStream Request_ms
        {
            get { return _request_ms; }
        }

        public BinaryWriter Request_bw
        {
            get { return _request_bw; }
        }

        public MemoryStream Response_ms
        {
            get { return _response_ms; }
        }

        public BinaryReader Response_br
        {
            get { return _response_br; }
        }

    }
}
