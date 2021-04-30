using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Helpers
{
    public static class StudioHttp
    {
        /// <summary>
        /// For example: StudioHttp.PostJsonAsync<LoginResult>("https://domain.com/api/user/login", requestData)
        /// </summary>
        public static async Task<TOutput> PostJsonAsync<TOutput>(string requestUri, object requestData)
        {
            var client = new HttpClient();

            var response = await client.PostAsJsonAsync<object>(requestUri, requestData);

            // test
            //string data = await response.Content.ReadAsStringAsync();

            TOutput output = await response.Content.ReadFromJsonAsync<TOutput>();

            return output;
        }

    }
}
