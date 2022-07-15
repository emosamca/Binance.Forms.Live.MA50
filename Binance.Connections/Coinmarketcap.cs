using Binance.Commons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Connections
{
    public static class Coinmarketcap
    {
        public static CoinmarketcapData GetDominances()
        {
            var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/global-metrics/quotes/latest");
            var client = new WebClient();
            client.Headers.Add("X-CMC_PRO_API_KEY", "81bb0e76-155b-48f3-814c-be6abe1556d3");
            client.Headers.Add("Accepts", "application/json");
            var domi = client.DownloadString(URL.ToString());
            var data = JsonConvert.DeserializeObject<CoinmarketcapState>(domi);
            return data.data;
        }
    }
}
