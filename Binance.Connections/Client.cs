using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Connections
{
    public class Client
    {
        private readonly HttpClient _httpClient;
        public readonly HttpClient _sapihttpClient;
        private string url = "https://api.binance.com/api/";
        private string sapiurl = "https://api.binance.com/sapi/";
        private string key = "";
        private string secret = "";


        public Client()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(url)

            };
            _httpClient.DefaultRequestHeaders
                 .Add("X-MBX-APIKEY", key);

            _httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            _sapihttpClient = new HttpClient
            {
                BaseAddress = new Uri(sapiurl)

            };
            _sapihttpClient.DefaultRequestHeaders
                 .Add("X-MBX-APIKEY", key);

            _sapihttpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


        }

        public Client(string apiKey, string apiSecret)
        {
            key = apiKey;
            secret = apiSecret;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(url)

            };
            _httpClient.DefaultRequestHeaders
                 .Add("X-MBX-APIKEY", key);

            _httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
        // GET
        public async Task<T> GetAsync<T>(string endpoint, string args = null)
        {
            var response = await _httpClient.GetAsync($"{endpoint}?{args}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.StatusCode.ToString());

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
        // GET
        public async Task<T> GetAsyncSapi<T>(string endpoint, string args = null)
        {
            var response = await _sapihttpClient.GetAsync($"{endpoint}?{args}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.StatusCode.ToString());

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
        //SIGNED GET
        public async Task<T> GetSignedAsync<T>(string endpoint, string args = null)
        {
            string headers = _httpClient.DefaultRequestHeaders.ToString();
            string timestamp = GetTimestamp();
            //if (args != null)
            args += "&timestamp=" + timestamp;
            //else
            //  args += "timestamp=" + timestamp;
            var signature = args.CreateSignature(secret);
            var response = await _httpClient.GetAsync($"{endpoint}?{args}&signature={signature}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.StatusCode.ToString());

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
        //SIGNED POST
        public async Task<T> PostSignedAsync<T>(string endpoint, string args = null)
        {
            string headers = _httpClient.DefaultRequestHeaders.ToString();
            string timestamp = GetTimestamp();
            args += "&timestamp=" + timestamp;


            var signature = args.CreateSignature(secret);
            var response = await _httpClient.PostAsync($"{endpoint}?{args}&signature={signature}", null);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.StatusCode.ToString());

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
        //SIGNED DELETE
        public async Task<T> DeleteSignedAsync<T>(string endpoint, string args = null)
        {
            string headers = _httpClient.DefaultRequestHeaders.ToString();
            string timestamp = GetTimestamp();
            args += "&timestamp=" + timestamp;

            var signature = args.CreateSignature(secret);
            var response = await _httpClient.DeleteAsync($"{endpoint}?{args}&signature={signature}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.StatusCode.ToString());

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }

        //Timestamp for signature
        private static string GetTimestamp()
        {
            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return milliseconds.ToString();
        }

    }
}
