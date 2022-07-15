using Binance.Commons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
namespace Binance.Socket.Connections
{
    public class SocketConnection
    {
        private List<coin> coinlist;
        private List<Prices> prices;
        private string startwss = "wss://stream.binance.com:9443/stream?streams=";
        WebSocket webSocket;
        bool okumaserbest;
        public bool IsAliveTest;
        public SocketConnection()
        {
            coinlist = new List<coin>();
            prices = new List<Prices>();
            okumaserbest = false;
        }
        public void SetCoinList(List<coin> coinliste, List<Prices> startprices )
        {
            coinlist = coinliste.DeepClone();
            foreach (var item in coinlist)
            {
                var fiyat = startprices.Find(x => x.Symbol == item.name);
                if (fiyat != null)
                    prices.Add(new Prices() { Symbol = item.name, Price = fiyat.Price });
            }
        }
        public void Start()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(startwss);
            foreach (var item in coinlist)
            {
                builder.Append(item.name.ToLowerInvariant() + "@miniTicker/");
            }
            string webaddr= builder.ToString().TrimEnd('/');
            webSocket = new WebSocket(webaddr);
            webSocket.OnMessage += WebSocket_OnMessage;
            webSocket.Connect();
            IsAliveTest = false;
        }

        public void Reconnect(List<coin> coinliste)
        {
            if(webSocket!=null)
            {
                if (webSocket.IsAlive)
                    webSocket.Close();
                webSocket.OnMessage -= WebSocket_OnMessage;
                webSocket = null;
            }
            coinlist = coinliste.DeepClone();
            StringBuilder builder = new StringBuilder();
            builder.Append(startwss);
            foreach (var item in coinlist)
            {
                builder.Append(item.name.ToLowerInvariant() + "@miniTicker/");
            }
            string webaddr = builder.ToString().TrimEnd('/');
            webSocket = new WebSocket(webaddr);
            webSocket.OnMessage += WebSocket_OnMessage;
            webSocket.Connect();
            IsAliveTest = false;

        }
        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            var sonuc = JsonConvert.DeserializeObject<streams>(e.Data);
            if (sonuc != null)
            {
                okumaserbest = false;
                if (prices.Find(x => x.Symbol == sonuc.data.s) != null)
                {
                    prices.Find(x => x.Symbol == sonuc.data.s).Price = sonuc.data.c;
                }
                else
                    prices.Add(new Prices() { Symbol = sonuc.data.s, Price = sonuc.data.c });
                okumaserbest = true;
            }
            IsAliveTest = true;
        }
        public double GetPrice(string coinname)
        {
            var sonuc = prices.Find(x => x.Symbol == coinname);
            if (sonuc != null)
                return sonuc.Price;
            return 0;
        }
        public void CoinEkle(string coinname, double startprice)
        {
            string subtext = "{\"method\":\"SUBSCRIBE\",\"params\":[\"" + coinname.ToLowerInvariant() + "@miniTicker\"],\"id\":1}";
            if (webSocket != null)
            {
                webSocket.Send(subtext);
                if(!prices.Any(x=>x.Symbol==coinname))
                    prices.Add(new Prices() { Symbol = coinname, Price = startprice });
            }
        }
        public void CoinSil(string coinname)
        {
            string subtext = "{\"method\":\"UNSUBSCRIBE\",\"params\":[\"" + coinname.ToLowerInvariant() + "@miniTicker\"],\"id\":1}";
            if (webSocket != null)
            {
                webSocket.Send(subtext);
                prices.RemoveAll(x => x.Symbol == coinname);
            }
        }
        public void Stop()
        {
            if(webSocket!=null)
                webSocket.Close();
        }
        
    }
}
