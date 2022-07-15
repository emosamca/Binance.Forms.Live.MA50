using Binance.Commons;
using Binance.Connections;
using Binance.Indicator;
using Binance.Mysql;
using Binance.Socket.Connections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Client;

namespace Binance.Forms
{
    public class AnalizWorker
    {
        public Client binanceClient;
        public Services binanceServices;
        private MysqlClass mysqlClass;
        private UserWorkers userWorkers;
        private TelegramBot bot;
        public int Yontem = 1;
        //private long crowgroupid = -599177208;
        private string publicIP;
        private bool systemStatusNormal = true;
        private SocketConnection socketConnection;
        //public bool IsTest = true;
        public AnalizWorker(UserWorkers userWorkers,TelegramBot bot )
        {
            GetPublicIp();
            HtmlHelper.PublicIp = publicIP;
            mysqlClass = new MysqlClass();
            binanceClient = new Client();
            binanceServices = new Services(binanceClient);
            this.userWorkers = userWorkers;
            this.bot = bot;
            this.bot.MessageReceived += Bot_MessageReceived;

        }

        private void StartExchangeInfoTask()
        {
            Task task = new Task(() => ExchangeInfoTask());
            task.Start();
        }
        public void ExchangeInfoTask()
        {
            int surem = 4;
            bool firsttime = false;
            int oncekidortsaat = DateTime.Now.Hour;
            oncekidortsaat++;
            while (oncekidortsaat != 2 && oncekidortsaat != 6 && oncekidortsaat != 10 && oncekidortsaat != 14 && oncekidortsaat != 18 && oncekidortsaat != 22)
            {
                oncekidortsaat++;
                if (oncekidortsaat == 24)
                    oncekidortsaat = 0;
            }

            while (true)
            {
                Debug.WriteLine($"Bir sonraki tarama saat : {oncekidortsaat}");
                try
                {
                    while (oncekidortsaat != DateTime.Now.Hour && !firsttime)
                    {
                        Thread.Sleep(15000);
                    }
                    Debug.WriteLine("ExchangeInfo Tarama başlıyor.");
                    if (!firsttime)
                    {
                        oncekidortsaat += surem;
                        if (oncekidortsaat >= 24)
                            oncekidortsaat = 2;
                    }
                    firsttime = false;
                    var sonuc = binanceServices.GetExchangeInfoFull();
                    var sonuc2 = sonuc.symbols.FindAll(x => x.IsTrading && x.quoteAsset == "BUSD"); // XXX USDT
                    sonuc2 = sonuc2.FindAll(x => !x.symbol.EndsWith("UPUSDT"));
                    sonuc2 = sonuc2.FindAll(x => !x.symbol.EndsWith("DOWNUSDT"));
                    List<string> yasaklist = new List<string> { "AUD", "BIDR", "BRL", "USDT", "EUR", "GBP", "RUB", "TRY", "TUSD", "USDC", "DAI", "IDRT", "UAH", "NGN", "VAI", "USDP", "UST", "BUSD", "SUSD", "USDP", "UST" };
                    var listem = sonuc2.FindAll(x => !yasaklist.Contains(x.baseAsset)).ToList();
                    List<coin> coinlist = new List<coin>();
                    foreach (var item in listem.OrderBy(x => x.symbol))
                    {
                        coinlist.Add(new coin() { name = item.symbol });
                    }
                    mysqlClass.SaveCoins(coinlist);
                    socketConnection.Reconnect(coinlist);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            
        }
        private void Bot_MessageReceived(object sender, TelegramMessageEventArgs e)
        {
            Debug.WriteLine($"{e.UserId}-{e.Messages}");
            bot.sendMessage($"Gelen mesaj: {e.Messages}", e.ChatId);
            try
            {
                string okunan = e.Messages;
                if (okunan == null)
                    return;
                string[] mesaj = okunan.Split(' ');
                List<string> mesajlist = mesaj.ToList();
                bot.sendMessage($"Gelen mesaj kelime sayısı: {mesajlist.Count}", e.ChatId);
                if (mesajlist.Count > 1)
                {
                    string uppermesaj = mesajlist[0].ToUpper();
                    if (uppermesaj == "EKLE")
                    {
                        string coin = mesajlist[1].ToUpperInvariant();
                        List<coin> coinlist = mysqlClass.GetCoinList(false);
                        if (coinlist.Any(x => x.name == coin && x.active==true))
                        {
                            bot.sendMessage($"Eklemek istediğiniz {coin} coini listede mevcut.", e.ChatId);
                            return;
                        }
                        else if (coinlist.Any(x => x.name == coin && x.active == false))
                        {
                            var coinbul = coinlist.Find(x => x.name == coin && x.active == false);
                            coinbul.active = true;
                            mysqlClass.UpdateCoinlist(coinbul);
                            bot.sendMessage($"Eklemek istediğiniz {coin} coin listeye eklendi.", e.ChatId);
                            Task.Delay(1000);
                            bot.sendMessage($"{e.UserId}) tarafından {coin} coini listeye eklendi.", e.ChatId);
                            var prices = binanceServices.GetTickerPrice(coin);
                            if (prices != null && prices.Count > 0)
                                socketConnection.CoinEkle(coin, prices[0].Price);
                            return;
                        }
                        var coininfo = binanceServices.GetSymbolInfo(coin);
                        Symbols sonuc = null;
                        if (coininfo != null)
                        {
                            sonuc = coininfo.Result;
                            if (sonuc == null)
                            {
                                bot.sendMessage($"Eklemek istediğiniz {coin} coin BINANCE de yok.", e.ChatId);
                                return;
                            }
                            if (sonuc.status == "TRADING")
                            {
                                bool eklenen = mysqlClass.SaveCoin(coin);
                                if (!eklenen)
                                    bot.sendMessage($"Eklemek istediğiniz {coin} coin bir hatadan eklenemedi.", e.ChatId);
                                else
                                {
                                    bot.sendMessage($"Eklemek istediğiniz {coin} coin listeye eklendi.", e.ChatId);
                                    Task.Delay(1000);
                                    bot.sendMessage($"{e.UserId}) tarafından {coin} coini listeye eklendi.", e.ChatId);
                                    var prices = binanceServices.GetTickerPrice(coin);
                                    if (prices != null && prices.Count > 0)
                                        socketConnection.CoinEkle(coin, prices[0].Price);
                                }
                            }
                            else
                            {
                                bot.sendMessage($"Eklemek istediğiniz {coin} coin TRADING bir coin olmadığından eklenmedi.", e.ChatId);
                            }
                        }
                    }
                    if (uppermesaj == "SIL" || uppermesaj == "SİL")
                    {
                        string coin = mesajlist[1].ToUpperInvariant();
                        if (!mysqlClass.IsThereAnyOpenOrder(coin))
                        {
                            List<coin> coinlist = mysqlClass.GetCoinList(true);
                            if (coinlist.Any(x => x.name == coin && x.active))
                            {
                                //mysqlClass.DeleteCoin(coin);
                                var coinim = coinlist.Find(x => x.name == coin && x.active);
                                coinim.active = false;
                                mysqlClass.UpdateCoinlist(coinim);
                                bot.sendMessage($"Silmek istediğiniz {coin} coini silinmiştir.", e.ChatId);
                                Task.Delay(1000);
                                bot.sendMessage($"{e.UserId}) tarafından {coin} coini silinmiştir.", e.ChatId);
                                socketConnection.CoinSil(coin);
                                return;
                            }
                            else
                            {
                                bot.sendMessage($"Silmek istediğiniz {coin} coin zaten listede yok.", e.ChatId);
                                return;
                            }
                        }

                    }
                    if (uppermesaj == "MTS")
                    {
                        double mtsal = Convert.ToDouble(mesajlist[1].Replace('.', ','));
                        double mtssat = Convert.ToDouble(mesajlist[2].Replace('.', ','));
                        MTS mts = mysqlClass.GetMTS();
                        mts.MTSAl = mtsal;
                        mts.MTSSat = mtssat;
                        mysqlClass.UpdateMTS(mts);
                        bot.sendMessage("MTS güncellendi", e.ChatId);
                        Task.Delay(1000);
                        bot.sendMessage($"{e.UserId}) tarafından MTS değerleri {mtsal.ToString("F3")} {mtssat.ToString("F3")} olarak güncellenmiştir.", e.ChatId);
                        //mts güncelle
                        return;
                    }
                    if (uppermesaj == "DURUMX")
                    {
                        string coinname = mesajlist[1].ToUpper();
                        var sonuc = mysqlClass.GetCoinDurum(coinname);
                        if (sonuc.Count > 0)
                        {
                            HtmlHelper.GetCoinDurum(sonuc);
                            bot.sendMessage($"http://{publicIP}/{coinname}.htm", e.ChatId);
                            Debug.WriteLine($"{e.UserId} tarafından {coinname} bilgileri alındı.");
                        }
                        else
                        {
                            bot.sendMessage($"{coinname} veritabanında bulunamamıştır.", e.ChatId);
                        }
                    }
                    if (uppermesaj == "BUTCE")
                    {
                        bot.sendMessage($"Gelen mesaj: BUTCE", e.ChatId);

                        if (mesajlist[1].ToUpper() == "TRADE")
                        {
                            bot.sendMessage($"Gelen mesaj: TRADE", e.ChatId);
                            if (mesajlist.Count != 6)
                            {
                                bot.sendMessage($"TRADE komutu örnek kullanımı: BUTCE TRADE 3 TIP ESIT|BOLU2 CUZDANID", e.ChatId);
                                return;
                            }
                            int tradesayi = 0;
                            try
                            {
                                tradesayi = Convert.ToInt32(mesajlist[2]);
                            }
                            catch (Exception ex)
                            {
                                bot.sendMessage($"TRADE sonrası parametre sayı olmalıdır.", e.ChatId);
                                return;
                            }
                            if (mesajlist[3].ToUpper() != "TIP")
                            {
                                bot.sendMessage($"TRADE komutu örnek kullanımı: BUTCE TRADE 3 TIP ESIT|BOLU2 CUZDANID", e.ChatId);
                                return;
                            }
                            if (mesajlist[4].ToUpper() != "ESIT" && mesajlist[4].ToUpper() != "BOLU2")
                            {
                                bot.sendMessage($"TRADE komutu örnek kullanımı: BUTCE TRADE 3 TIP ESIT|BOLU2 CUZDANID", e.ChatId);
                                return;
                            }
                            int? cuzdanid = null;
                            try
                            {
                                cuzdanid = Convert.ToInt32(mesajlist[5]);
                            }
                            catch(Exception ex)
                            {
                                bot.sendMessage($"CUZDANID sayı olmalıdır.", e.ChatId);
                                return;
                            }
                            if (cuzdanid != null)
                            {
                                var userworker = userWorkers[e.UserId];
                                if (userworker != null)
                                    userworker.SetButce(tradesayi, mesajlist[4], e.ChatId,(int)cuzdanid);
                            }
                        }
                        if(mesajlist[1].ToUpper()=="EKLE")
                        {
                            if (mesajlist.Count != 4)
                            {
                                bot.sendMessage($"EKLE komutu örnek kullanımı: BUTCE EKLE 1000 CUZDANID", e.ChatId);
                                return;
                            }
                            int eklenecek = 0;
                            try
                            {
                                eklenecek = Convert.ToInt32(mesajlist[2]);
                            }
                            catch (Exception ex)
                            {
                                bot.sendMessage($"EKLE sonrası parametre sayı olmalıdır.", e.ChatId);
                                return;
                            }
                            int? cuzdanid = null;
                            try
                            {
                                cuzdanid = Convert.ToInt32(mesajlist[3]);
                            }
                            catch (Exception ex)
                            {
                                bot.sendMessage($"CUZDANID sayı olmalıdır.", e.ChatId);
                                return;
                            }
                            if (cuzdanid != null)
                            {

                                var userworker = userWorkers[e.UserId];
                                if (userworker != null)
                                    userworker.ButceEkle(eklenecek, e.ChatId, (int)cuzdanid);
                            }
                        }
                        if (mesajlist[1].ToUpper() == "CEK")
                        {
                            if (mesajlist.Count != 4)
                            {
                                bot.sendMessage($"CEK komutu örnek kullanımı: BUTCE CEK 1000 CUZDANID", e.ChatId);
                                return;
                            }
                            int silinecek = 0;
                            try
                            {
                                silinecek = Convert.ToInt32(mesajlist[2]);
                            }
                            catch (Exception ex)
                            {
                                bot.sendMessage($"CEK sonrası parametre sayı olmalıdır.", e.ChatId);
                                return;
                            }
                            int? cuzdanid = null;
                            try
                            {
                                cuzdanid = Convert.ToInt32(mesajlist[3]);
                            }
                            catch (Exception ex)
                            {
                                bot.sendMessage($"CUZDANID sayı olmalıdır.", e.ChatId);
                                return;
                            }
                            if (cuzdanid != null)
                            {
                                var userworker = userWorkers[e.UserId];
                                if (userworker != null)
                                    userworker.ButceSil(silinecek, e.ChatId,(int)cuzdanid);
                            }
                        }
                    }
                    if(uppermesaj=="SAT")
                    {
                        if(mesajlist.Count!=3)
                        {
                            bot.sendMessage($"SAT komutu örnek kullanımı: SAT COINADI|HEPSI CUZDANID", e.ChatId);
                            return;
                        }
                        int? cuzdanid = null;
                        try
                        {
                            cuzdanid = Convert.ToInt32(mesajlist[2]);
                        }
                        catch (Exception ex)
                        {
                            bot.sendMessage($"CUZDANID sayı olmalıdır.", e.ChatId);
                            return;
                        }
                        if (cuzdanid != null)
                        {

                            if (mesajlist[1].ToUpper() == "HEPSI")
                            {
                                bot.sendMessage($"Gelen mesaj: HEPSI", e.ChatId);
                                var userworker = userWorkers[e.UserId];
                                if (userworker != null)
                                    userworker.SellCoins(e.ChatId,(int)cuzdanid);
                            }
                            else
                            {
                                var userworker = userWorkers[e.UserId];
                                if (userworker != null)
                                    userworker.SellCoins(e.ChatId, (int)cuzdanid, mesajlist[1].ToUpper());
                            }
                        }

                    }
                }
                if (mesajlist.Count == 1)
                {
                    string uppermesaj = mesajlist[0].ToUpper();
                    if (uppermesaj == "MTS")
                    {
                        Debug.WriteLine("MTS girildi.");
                        MTS mts = mysqlClass.GetMTS();
                        string mess = $"MTS {mts.MTSAl.ToString("F3")} {mts.MTSSat.ToString("F3")}";
                        bot.sendMessage(mess, e.ChatId);
                        return;
                    }
                    if (uppermesaj == "PNL")
                    {
                        var userworker = userWorkers[e.UserId];
                        if(userworker!=null)
                            userworker.PNLhesap();
                    }
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.ToString());
            }
        }

        public void StartAnaliz()
        {
            var btcstates = mysqlClass.GetBTCma50states();
            GlobalVars.btc50up = btcstates.btcma50up == 1 ? true : false;
            GlobalVars.btc20up = btcstates.btcma20up == 1 ? true : false;
            GlobalVars.BotFullStop = btcstates.botfullstop;
            GlobalVars.OnlyBuyStop = btcstates.onlybuystop;
            GlobalVars.OncekiBotFullStop = btcstates.botfullstop;
            GlobalVars.OncekiOnlyBuyStop = btcstates.onlybuystop;

            GlobalVars.CandleCount = mysqlClass.GetCandleCount();
            //StartStartStopTask();
            GetSystemStatus();

            StartWebSocket();

            StartLMTSTaramaTask();
            //StartLMTSHesaplayiciTask();
            StartTargetReachedTask();
            if (!GlobalVars.Istest)
            {
                StartPNLTask();
            }
            StartSystemStatusTask();
            StartSatEmirTask();
            StartWebSocketAliveTest();
            StartExchangeInfoTask();
        }
        private void StartWebSocketAliveTest()
        {
            Task task = new Task(() => WebSocketAliveTest());
            task.Start();

        }
        private void WebSocketAliveTest()
        {
            while (true)
            {
                try
                {

                    Thread.Sleep(5000);
                    socketConnection.IsAliveTest = false;
                    Thread.Sleep(5000);
                    if (!socketConnection.IsAliveTest)
                    {
                        var coinlist = mysqlClass.GetCoinList(true);
                        Debug.WriteLine("Websocket Reconnect oldu");
                        socketConnection.Reconnect(coinlist);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
        public void StartWebSocket()
        {
            socketConnection = new SocketConnection();
            var coinlist = mysqlClass.GetCoinList(true);
            var prices = binanceServices.GetTickerPrice();
            socketConnection.SetCoinList(coinlist, prices);
            socketConnection.Start();
            foreach (var item in userWorkers.userWorkers)
            {
                item.socketConnection = socketConnection;
            }
        }
        private void GetSystemStatus()
        {
            var system = binanceServices.GetSystemStatus();
            if (system == null)
                systemStatusNormal = false;
            if (system.status == 1)
                systemStatusNormal = false;
            else
                systemStatusNormal = true;
            foreach (var item in userWorkers.userWorkers)
            {
                item.systemStatusNormal = systemStatusNormal;
            }
        }

        private void StartSatEmirTask()
        {
            Task task = new Task(() => SatEmirTask());
            task.Start();
        }
        private void SatEmirTask()
        {
            while (true)
            {
                try
                {

                    Thread.Sleep(1000);
                    var weborders = mysqlClass.GetWebOrders(WebOrderState.Open);
                    //Debug.WriteLine($"{weborders.Count} adet emir var.");
                    foreach (var item in weborders)
                    {
                        var userworker = userWorkers[item.userid];
                        if (userworker != null)
                            userworker.WebMesajIsle(item);
                        else
                        {
                            item.state = WebOrderState.Error;
                            mysqlClass.UpdateWebOrdersState(item);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        private void StartSystemStatusTask()
        {
            Task task = new Task(() => SystemStatusTask());
            task.Start();
        }
        private void SystemStatusTask()
        {
            int oncekidakika = DateTime.Now.Minute;
            while (true)
            {
                try
                {
                    while (oncekidakika != DateTime.Now.Minute)
                    {
                        Thread.Sleep(5000);
                    }
                    GetSystemStatus();
                    oncekidakika++;
                    if (oncekidakika == 60)
                        oncekidakika = 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
        private void GetPublicIp()
        {
            string url = "http://checkip.dyndns.org";
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];
            publicIP = a4.Trim();
        }
        private void StartLMTSTaramaTask()
        {
            Task task = new Task(() => LMTSTaramaTask());
            task.Start();
        }
        public void LMTSTaramaTask()
        {
            int surem = 4;
            bool firsttime = false;
            int oncekidortsaat = DateTime.Now.Hour;
            oncekidortsaat++;
            while (oncekidortsaat!=3 && oncekidortsaat!=7 && oncekidortsaat!=11 && oncekidortsaat!=15 && oncekidortsaat!=19 && oncekidortsaat!=23)
            {
                oncekidortsaat++;
                if (oncekidortsaat == 24)
                    oncekidortsaat = 0;
            }
            
            while (true)
            {
                Debug.WriteLine($"Bir sonraki tarama saat : {oncekidortsaat}");
                try
                {
                    while (oncekidortsaat != DateTime.Now.Hour && !firsttime)
                    {
                        Thread.Sleep(5000);
                    }
                    Debug.WriteLine("Tarama başlıyor.");
                    Thread.Sleep(2000);
                    if (!firsttime)
                    {
                        oncekidortsaat += surem;
                        if (oncekidortsaat >= 24)
                            oncekidortsaat = 3;
                    }
                    if (!systemStatusNormal)
                        continue;
                    List<coin> coins = new List<coin>();

                    coins = mysqlClass.GetCoinList(true);
                    List<string> mumsure = new List<string>();
                    mumsure.Add("4h");
                    if (DateTime.Now.Hour == 3)
                        mumsure.Add("1d");

                    foreach (var sure in mumsure)
                    {
                        Task<List<WorkerReturnObjOguslu>>[] tasks = new Task<List<WorkerReturnObjOguslu>>[25];
                        List<List<coin>> analiste = new List<List<coin>>();
                        for (int i = 0; i < 25; i++)
                        {
                            List<coin> liste = new List<coin>();
                            analiste.Add(liste);
                        }
                        int konum = 0;
                        foreach (var item in coins)
                        {
                            analiste[konum].Add(item);
                            konum++;
                            if (konum == 25)
                                konum = 0;
                        }

                        MTS mts = mysqlClass.GetMTS();
                        for (int i = 0; i < tasks.Length; i++)
                        {
                            if (analiste[i].Count == 0)
                                continue;
                            List<coin> copylist = analiste[i];//.DeepClone();
                                                              //Debug.WriteLine($"{i} dip başlıyor.");
                            var clientim = new Client();
                            var servisim = new Services(clientim);
                            tasks[i] = Task<List<WorkerReturnObjOguslu>>.Factory.StartNew(() => LMTSTaramaThread(servisim, copylist, new List<string>() { sure }, mts), TaskCreationOptions.LongRunning);
                        }
                        List<Task<List<WorkerReturnObjOguslu>>> tasks1 = new List<Task<List<WorkerReturnObjOguslu>>>();
                        foreach (var item in tasks)
                        {
                            if (item != null)
                                tasks1.Add(item);
                        }
                        //Debug.WriteLine($"{tasks1.Count} adet dip task bekleniyor.");
                        Task.WaitAll(tasks1.ToArray());
                        List<NewBuyTable> alimlistesi = new List<NewBuyTable>();
                        List<NewBuyTable> satimlistesi = new List<NewBuyTable>();
                        foreach (var item in tasks1)
                        {
                            var sonuc = item.Result;
                            if (sonuc == null)
                                continue;
                            foreach (var result in sonuc)
                            {
                                if (result.Alinacak != null)
                                    alimlistesi.Add(result.Alinacak);
                                if (result.Satilacak != null)
                                    satimlistesi.Add(result.Satilacak);
                            }
                        }

                        if (Yontem == 1)
                            userWorkers.DoBuyOrSell(alimlistesi, satimlistesi,sure);
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

        }

        private List<WorkerReturnObjOguslu> LMTSTaramaThread(Services binanceServices, List<coin> Coinlist, List<string> interval, MTS mts)
        {
            List<WorkerReturnObjOguslu> workerReturnObjOguslulist = new List<WorkerReturnObjOguslu>();

            foreach (var sure in interval)
            {
                foreach (var coin in Coinlist)
                {
                    var hesap = LMTSTarama(coin, sure, binanceServices, true);
                    if (hesap != null)
                        workerReturnObjOguslulist.Add(hesap);
                }
            }
            return workerReturnObjOguslulist;
        }

        private void StartTargetReachedTask()
        {
            Task task = new Task(() => TargetReachedTask());
            task.Start();
        }
        private void TargetReachedTask()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(5000);

                            userWorkers.TargetReached( systemStatusNormal);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        private WorkerReturnObjOguslu LMTSTarama(coin coin, string sure, Services binanceservice, bool taramatask)
        {
            NewBuyTable LMTScoin = new NewBuyTable();
            LMTScoin.CoinName = coin.name;
            WorkerReturnObjOguslu workerreturncalismaobj = new WorkerReturnObjOguslu();
            var kline = binanceservice.GetKlines(coin.name, sure, 52);

            if (kline == null)
            {
                return null;
            }
            if (kline.Count < 52)
            {
                return null;
            }
            if(taramatask)
                if (kline.Last().CloseTime > DateHelper.GetCurrentTimeStam())
                    kline.RemoveAt(kline.Count - 1);

            bool alalimmi = false;
            WMA wma = new WMA(50);
            wma.Load(kline);
            var wma50 = wma.Calculate();
            var sonmum = kline.Last();
            var sondanbironcekimum = kline[kline.Count - 2];
            var wma50sonmum = wma50.Values.Last();
            var wma50sondanbironceki = wma50.Values[wma50.Values.Count - 2];

            bool satalimmi = false;
            if(((double)sonmum.Close>wma50sonmum && sonmum.Open<wma50sonmum)  || ((double)sondanbironcekimum.Close<wma50sondanbironceki && sonmum.Open>wma50sonmum && (double)sonmum.Close>wma50sonmum)) 
            {
                alalimmi = true;
            }
            if (((double)sonmum.Close < wma50sonmum && sonmum.Open > wma50sonmum) || ((double)sondanbironcekimum.Close > wma50sondanbironceki && sonmum.Open < wma50sonmum && (double)sonmum.Close < wma50sonmum))
            {
                satalimmi = true;
            }
            if (alalimmi)
            {
                LMTScoin.State = 1;
                workerreturncalismaobj.Alinacak = LMTScoin;
            }
            else if(satalimmi)
                workerreturncalismaobj.Satilacak = LMTScoin;
            else
            {
                workerreturncalismaobj.Alinacak = null;
                workerreturncalismaobj.Satilacak = null;
            }

            return workerreturncalismaobj;
        }



        private void StartPNLTask()
        {
            Task task = new Task(() => StartPNL());
            task.Start();

        }
        private void StartPNL()
        {
            int pnloncekisaat = DateTime.Now.Hour;

            while (true)
            {
                try
                {
                    while (pnloncekisaat == DateTime.Now.Hour)
                    {
                        Thread.Sleep(5000);
                    }
                    pnloncekisaat = DateTime.Now.Hour;
                    foreach (var item in userWorkers.userWorkers)
                    {
                        Task task = new Task(() => item.PNLhesap());
                        task.Start();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}
