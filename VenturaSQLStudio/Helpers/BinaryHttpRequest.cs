using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Helpers
{
    public static class BinaryHttpRequest
    {
        public static async Task<byte[]> RequestAsync(string url_string, MemoryStream memorystream)
        {
            memorystream.Position = 0;

            // Send the request
            Uri uri = new Uri(url_string);

            StreamContent content = new StreamContent(memorystream);

            HttpClient client = new HttpClient();

            using (HttpResponseMessage response = await client.PostAsync(uri, content))
            {
                response.EnsureSuccessStatusCode();

                byte[] response_array = await response.Content.ReadAsByteArrayAsync();

                return response_array;
            }

        }

        public static byte[] Request(string url_string, MemoryStream memorystream)
        {
            memorystream.Position = 0;

            // Send the request
            Uri uri = new Uri(url_string);

            StreamContent content = new StreamContent(memorystream);

            HttpClient client = new HttpClient();

            using (HttpResponseMessage response = client.PostAsync(uri, content).Result)
            {
                response.EnsureSuccessStatusCode();

                byte[] response_array = response.Content.ReadAsByteArrayAsync().Result;

                return response_array;
            }

        }



    } // class
} // namespace