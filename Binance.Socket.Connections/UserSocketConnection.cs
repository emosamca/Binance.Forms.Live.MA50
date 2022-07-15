using Binance.Commons;
using Newtonsoft.Json;
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
    public class UserSocketConnection
    {
        private HttpClient _httpClient;
        private string url = "https://api.binance.com/";
        private string websocketaddr = "wss://stream.binance.com:9443/stream?streams=";
        private string ListenKey;
        private string mywebsocketaddr;
        WebSocket Socket;
        WebSocket ws;
        public string Key { get; set; }
        public string SecretKey { get; set; }

        public event EventHandler<UserSocketConnectionMessageEventArgs> MessageReceived;
        public UserSocketConnection(string key, string secretkey)
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
            var task = Task.Run(async () => await _httpClient.PostAsync("api/v3/userDataStream", null));
            dynamic result = task.Result;
            var mesaj = task.Result.Content.ReadAsStringAsync();
            var sonuc = JsonConvert.DeserializeObject<UserDataStreamResponse>(mesaj.Result.ToString());
            ListenKey = sonuc.ListenKey;
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
                    Debug.WriteLine("Listenkey sıfırlanacak.");
                    
                    var task = Task.Run(async () => await _httpClient.PostAsync("api/v3/userDataStream", null));
                    dynamic result = task.Result;
                    var mesaj = task.Result.Content.ReadAsStringAsync();
                    var sonuc = JsonConvert.DeserializeObject<UserDataStreamResponse>(mesaj.Result.ToString());
                    ListenKey = sonuc.ListenKey;
                    mywebsocketaddr = websocketaddr + ListenKey;
                    ws = new WebSocket(mywebsocketaddr);

                    //ws.OnMessage += Ws_OnMessage; ;
                    ws.Connect(); Debug.WriteLine("Listenkey başarıyla sıfırlandı.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Sıfırlamada hata:{ex.ToString()}");
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
                    Debug.WriteLine("Listenkey yenilenecek.");
                    var task = Task.Run(async () => await _httpClient.PutAsync($"api/v3/userDataStream?listenKey={ListenKey}", null));
                    dynamic result = task.Result;
                    Debug.WriteLine("Listenkey başarıyla yenilendi.");
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Yenilemede hata:{ex.ToString()}");
                }
            }
        }
        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            Debug.WriteLine("Ham mesaj geldi.");
            if(e.IsText)
            {
                var sonuc = JsonConvert.DeserializeObject<userstreams>(e.Data);
                MessageReceived?.Invoke(this, new UserSocketConnectionMessageEventArgs() { stream = sonuc });
            }
        }
    }
}
