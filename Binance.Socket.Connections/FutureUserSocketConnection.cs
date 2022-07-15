using Binance.Commons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Binance.Socket.Connections
{
    public class FutureUserSocketConnection
    {
        private HttpClient _httpClient;
        private string url = "https://fapi.binance.com/";
        private string websocketaddr = "wss://fstream.binance.com/ws/";
        private string ListenKey;
        private string mywebsocketaddr;
        WebSocket Socket;
        WebSocket ws;
        public string Key { get; set; }
        public string SecretKey { get; set; }

        public event EventHandler<futureUserSocketConnMessEventArgs> MessageReceived;
        public FutureUserSocketConnection(string key, string secretkey)
        {
            Key = key;
            SecretKey = secretkey;
        }

        private void CreateClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(url)

            };
            _httpClient.DefaultRequestHeaders
                 .Add("X-MBX-APIKEY", Key);

            _httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
        public void Start()
        {
            CreateClient();
            var task = Task.Run(async () => await _httpClient.PostAsync("fapi/v1/listenKey", null));
            dynamic result = task.Result;
            var mesaj = task.Result.Content.ReadAsStringAsync();
            var sonuc = JsonConvert.DeserializeObject<UserDataStreamResponse>(mesaj.Result.ToString());
            ListenKey = sonuc.ListenKey;
            Debug.WriteLine($"ListenKey:{ListenKey}");
            mywebsocketaddr = websocketaddr + ListenKey;
            ws = new WebSocket(mywebsocketaddr);

            ws.OnMessage += Ws_OnMessage; ;
            ws.Connect();
            Task tazelemetask = new Task(() => ListenKeyYenile());
            tazelemetask.Start();

            Task sifirlatask = new Task(() => ListenKeySifirla());
            sifirlatask.Start();
        }
        private void ListenKeySifirla()
        {
            while (true)
            {
                try
                {
                    // 30 dakika
                    Thread.Sleep(360 * 60 * 1000);
                    Debug.WriteLine("FListenkey sıfırlanacak.");

                    var task = Task.Run(async () => await _httpClient.PostAsync("fapi/v1/listenKey", null));
                    dynamic result = task.Result;
                    var mesaj = task.Result.Content.ReadAsStringAsync();
                    var sonuc = JsonConvert.DeserializeObject<UserDataStreamResponse>(mesaj.Result.ToString());
                    ListenKey = sonuc.ListenKey;
                    mywebsocketaddr = websocketaddr + ListenKey;
                    ws = new WebSocket(mywebsocketaddr);

                    //ws.OnMessage += Ws_OnMessage; ;
                    ws.Connect(); Debug.WriteLine("FListenkey başarıyla sıfırlandı.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"FSıfırlamada hata:{ex.ToString()}");
                }
            }
        }
        private void ListenKeyYenile()
        {
            while (true)
            {
                try
                {
                    // 30 dakika
                    Thread.Sleep(30 * 60 * 1000);
                    Debug.WriteLine("FListenkey yenilenecek.");
                    var task = Task.Run(async () => await _httpClient.PutAsync("fapi/v1/listenKey", null));
                    dynamic result = task.Result;
                    Debug.WriteLine("FListenkey başarıyla yenilendi.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"FYenilemede hata:{ex.ToString()}");
                }
            }
        }
        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            Debug.WriteLine("FHam mesaj geldi.");
            Debug.WriteLine(e.Data);
            Debug.WriteLine(e.IsText);
            if (e.IsText)
            {
                Debug.WriteLine("Istext.gidiyorum.");
                MessageReceived?.Invoke(this, new futureUserSocketConnMessEventArgs() { stream= e.Data });
            }
        }
    }
}