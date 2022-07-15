//#define DEBUGMODE
using Binance.Commons;
using Binance.Connections;
using Binance.Indicator;
using Binance.Mysql;
using Binance.Socket.Connections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class UserWorker
    {
        public Client binanceClient;
        public Services binanceServices;
        private MysqlClass mysqlClass;
        private TelegramBot bot;
        //private long crowgroupid = -599177208;
        public List<UsersBudget> usersBudgets;
        public List<UsersBudget> AtYarisiBudget;
        public User user;
        private object lockobj = new object();
        private object malock = new object();
        private object pnllockobj = new object();
        public bool systemStatusNormal;
        public SocketConnection socketConnection;
        public UserSocketConnection userSocketConnection;
        public FutureUserSocketConnection futureUserSocketConnection;
        public UserWorker(User user, TelegramBot telegramBot)
        {
            this.user = user;
            bot = telegramBot;
            binanceClient = new Client(user.Apikey, user.Apisecretkey);
            binanceServices = new Services(binanceClient);
            mysqlClass = new MysqlClass();
            usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
            userSocketConnection = new UserSocketConnection(user.Apikey, user.Apisecretkey);
            userSocketConnection.MessageReceived += UserSocketConnection_MessageReceived;
            userSocketConnection.Start();

            futureUserSocketConnection = new FutureUserSocketConnection(user.Apikey, user.Apisecretkey);
            futureUserSocketConnection.MessageReceived += FutureUserSocketConnection_MessageReceived;
            futureUserSocketConnection.Start();

        }

        private void FutureUserSocketConnection_MessageReceived(object sender, futureUserSocketConnMessEventArgs e)
        {
            Debug.WriteLine($"FYeni mesaj geldi.{e.stream}");
            var s = JObject.Parse(e.stream);
            if (s["e"].ToString() == "ORDER_TRADE_UPDATE")
            {
                var o = s["o"];
                var sonuc2 = JsonConvert.DeserializeObject<futureOrderDetails>(o.ToString());
                string message = $"Future {sonuc2.s} için {sonuc2.S} emri {sonuc2.x}-{sonuc2.X}. Fiyat:{sonuc2.p}-Adet:{sonuc2.q}";
                bot.sendMessage(message, user.Groupid);
            }
        }

        private void UserSocketConnection_MessageReceived(object sender, UserSocketConnectionMessageEventArgs e)
        {
            Debug.WriteLine($"Yeni mesaj geldi.{e.stream.data.e}");
           if(e.stream.data.e== "executionReport")
            {
                var datam = e.stream.data;
                string message = $"{datam.s} için {datam.S} emri {datam.x}-{datam.X}. Fiyat:{datam.p}-Adet:{datam.q}";
                bot.sendMessage(message, user.Groupid);
            }
           else if(e.stream.data.e== "outboundAccountPosition")
            {
                var datam = e.stream.data;
                foreach (var item in datam.B)
                {
                    string message = $"{item.a} free {item.f} locked {item.l}";
                    bot.sendMessage(message, user.Groupid);
                }
            }
           else if(e.stream.data.e== "balanceUpdate")
            {
                var datam = e.stream.data;
                string message = $"{datam.a} için delta {datam.d}";
                bot.sendMessage(message, user.Groupid);

            }
        }

        public double GetFiyat(string coinname, UsersBudget budget)
        {
            double fiyat = 0;
            if(budget.IsAlive==0)
            {
                fiyat = socketConnection.GetPrice(coinname);
            }
            return fiyat;
        }
        public void WebMesajIsle(WebOrder webOrder)
        {
            string testmesaj = ""; 
            if (webOrder.emir_kod == WebOrderCode.Sell)  // sat emri
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Sat emri geldi.");
                    var newbuytables = mysqlClass.GetNewBuyTables(webOrder.newbuytableid);
                    Debug.WriteLine($"{newbuytables.Count} adet emir mevcut.");
                    if (newbuytables != null)
                    {
                        var satilacak = newbuytables.Find(x => x.Id == webOrder.newbuytableid);
                        if (satilacak != null)
                        {
                            Debug.WriteLine($"{satilacak.Id} bulundu.");
                            usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                            if (usersBudgets != null)
                            {
                                Debug.WriteLine($"{usersBudgets.Count} cüzdan bulundu.");
                                var budget = usersBudgets.Find(x => x.BudgetName == satilacak.UserName);
                                if (budget != null)
                                {
                                    Debug.WriteLine($"{budget.BudgetName} cüzdan bulundu.");
                                    Order order = binanceServices.CreateMarketSellOrder(satilacak.CoinName, satilacak.TotalAdet, budget, GetFiyat(satilacak.CoinName,budget));
                                    testmesaj=budget.IsAlive==0 ? "(Test)" : "";
                                    if (order == null)
                                    {
                                        bot.sendMessage($"{testmesaj} {satilacak.CoinName} satışı için emir oluşturulamadı.", user.Groupid);
                                        webOrder.state = WebOrderState.Error;
                                        mysqlClass.UpdateWebOrdersState(webOrder);
                                        return;
                                    }
                                    satilacak.Sell(order.cummulativeQuoteQty / satilacak.TotalAdet, "Kullanıcı", 0);
                                    mysqlClass.UpdateNewBuyTable(satilacak);
                                    bot.sendMessage($"{testmesaj}(coin sat) {satilacak.CoinName} satıldı. Fiyat={(order.cummulativeQuoteQty / satilacak.TotalAdet).ToString("F8")}-Adet={satilacak.TotalAdet}-K/Z=%{satilacak.SellProfit.ToString("F3")}", user.Groupid);
                                    if (satilacak.TotalAdet > 0)
                                        budget.CoinSelled(order.cummulativeQuoteQty);
                                }
                                mysqlClass.UpdateUserBadget(budget);
                            }
                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.SellBudget)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Cüzdan Sat emri geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        var budget = usersBudgets.Find(x => x.Id == webOrder.newbuytableid);
                        if (budget != null)
                        {
                            Debug.WriteLine($"{budget} cüzdan bulundu.");
                            var emirler = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                            foreach (var item in emirler)
                            {
                                Order order = binanceServices.CreateMarketSellOrder(item.CoinName, item.TotalAdet, budget, GetFiyat(item.CoinName, budget));
                                testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                                if (order == null)
                                {
                                    bot.sendMessage($"{testmesaj} {item.CoinName} satışı için emir oluşturulamadı.", user.Groupid);
                                    webOrder.state = WebOrderState.Error;
                                    mysqlClass.UpdateWebOrdersState(webOrder);
                                    return;
                                }
                                item.Sell(order.cummulativeQuoteQty / item.TotalAdet, "Kullanıcı", 0);
                                mysqlClass.UpdateNewBuyTable(item);
                                bot.sendMessage($"{testmesaj}(Cüzdan sat) {item.CoinName} satıldı. Fiyat={(order.cummulativeQuoteQty / item.TotalAdet).ToString("F8")}-Adet={item.TotalAdet}-K/Z=%{item.SellProfit.ToString("F3")}", user.Groupid);
                                if (item.TotalAdet > 0)
                                    budget.CoinSelled(order.cummulativeQuoteQty);
                            }
                            mysqlClass.UpdateUserBadget(budget);
                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.SellAll)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Tüm Cüzdan Sat emri geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        foreach (var budget in usersBudgets)
                        {
                            Debug.WriteLine($"{budget.BudgetName} cüzdan bulundu.");
                            var emirler = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                            foreach (var emir in emirler)
                            {
                                Order order = binanceServices.CreateMarketSellOrder(emir.CoinName, emir.TotalAdet, budget, GetFiyat(emir.CoinName, budget));
                                testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                                if (order == null)
                                {
                                    bot.sendMessage($"{testmesaj} {emir.CoinName} satışı için emir oluşturulamadı.", user.Groupid);
                                    webOrder.state = WebOrderState.Error;
                                    mysqlClass.UpdateWebOrdersState(webOrder);
                                    return;
                                }
                                emir.Sell(order.cummulativeQuoteQty / emir.TotalAdet, "Kullanıcı", 0);
                                mysqlClass.UpdateNewBuyTable(emir);
                                bot.sendMessage($"{testmesaj}(Tüm cüzdanlar) {emir.CoinName} satıldı. Fiyat={(order.cummulativeQuoteQty / emir.TotalAdet).ToString("F8")}-Adet={emir.TotalAdet}-K/Z=%{emir.SellProfit.ToString("F3")}", user.Groupid);
                                if (emir.TotalAdet > 0)
                                    budget.CoinSelled(order.cummulativeQuoteQty);

                                mysqlClass.UpdateUserBadget(budget);
                            }
                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.TradeSet)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Cüzdan Trade Set emri geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        foreach (var budget in usersBudgets)
                        {
                            Debug.WriteLine($"{budget.BudgetName} cüzdan bulundu.");
                            if (budget.Id == webOrder.newbuytableid)
                            {
                                testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                                int eski = budget.TradeMax;
                                budget.TradeMax = (int)((double)webOrder.Parameter);
                                mysqlClass.UpdateUserBadgetTrade(budget);
                                bot.sendMessage($"{testmesaj}{budget.BudgetName} için trade max güncellendi. Eski {eski} yeni {budget.TradeMax}", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                break;
                            }

                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.TradeType)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Cüzdan Trade Type emri geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        foreach (var budget in usersBudgets)
                        {
                            Debug.WriteLine($"{budget.BudgetName} cüzdan bulundu.");
                            if (budget.Id == webOrder.newbuytableid)
                            {
                                testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                                int eski = budget.TradeTytpe;
                                budget.TradeTytpe = (int)((double)webOrder.Parameter);
                                mysqlClass.UpdateUserBadgetTrade(budget);
                                bot.sendMessage($"{testmesaj}{budget.BudgetName} için trade type güncellendi. Eski {eski} yeni {budget.TradeTytpe}", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                break;
                            }

                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.SellCoinAll)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("All Cüzdan Coin Sat emri geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        string coinname = "";

                        foreach (var budget in usersBudgets)
                        {
                            Debug.WriteLine($"{budget} cüzdan bulundu.");
                            var emirler = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                            foreach (var item in emirler)
                            {
                                if (item.Id == webOrder.newbuytableid)
                                {
                                    coinname = item.CoinName;
                                    break;
                                }
                            }
                            if (coinname != "")
                                break;
                        }
                        if (coinname != null)
                        {
                            foreach (var budget in usersBudgets)
                            {
                                var emirler = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                                foreach (var emir in emirler)
                                {
                                    testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                                    if (emir.CoinName == coinname)
                                    {
                                        Order order = binanceServices.CreateMarketSellOrder(emir.CoinName, emir.TotalAdet, budget, GetFiyat(emir.CoinName, budget));
                                        if (order == null)
                                        {
                                            bot.sendMessage($"{testmesaj} {emir.CoinName} satışı için emir oluşturulamadı.", user.Groupid);
                                            webOrder.state = WebOrderState.Error;
                                            mysqlClass.UpdateWebOrdersState(webOrder);
                                            return;
                                        }
                                        emir.Sell(order.cummulativeQuoteQty / emir.TotalAdet, "Kullanıcı", 0);
                                        mysqlClass.UpdateNewBuyTable(emir);
                                        bot.sendMessage($"{testmesaj}(Tüm Cüzdan coin sat) {emir.CoinName} satıldı. Fiyat={(order.cummulativeQuoteQty / emir.TotalAdet).ToString("F8")}-Adet={emir.TotalAdet}-K/Z=%{emir.SellProfit.ToString("F3")}", user.Groupid);
                                        if (emir.TotalAdet > 0)
                                            budget.CoinSelled(order.cummulativeQuoteQty);
                                    }
                                }
                                mysqlClass.UpdateUserBadget(budget);
                            }

                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.LockCoin)
            {
                Debug.WriteLine("Coin lock emri geldi.");
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                if (usersBudgets != null)
                {
                    foreach (var budget in usersBudgets)
                    {
                        Debug.WriteLine($"{budget} cüzdan bulundu.");
                        var emirler = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                        testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                        foreach (var item in emirler)
                        {
                            if (item.Id == webOrder.newbuytableid)
                            {
                                item.IsLocked = true;
                                mysqlClass.UpdateNewBuyTable(item);
                                bot.sendMessage($"{testmesaj}(Coin kilitle) {item.CoinName} kilitlendi.", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                return;
                            }
                        }
                    }
                }
                webOrder.state = WebOrderState.Close;
                mysqlClass.UpdateWebOrdersState(webOrder);
            }
            else if (webOrder.emir_kod == WebOrderCode.UnLockCoin)
            {
                Debug.WriteLine("Coin unlock emri geldi.");
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                if (usersBudgets != null)
                {
                    foreach (var budget in usersBudgets)
                    {
                        Debug.WriteLine($"{budget} cüzdan bulundu.");
                        var emirler = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                        testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                        foreach (var item in emirler)
                        {
                            if (item.Id == webOrder.newbuytableid)
                            {
                                item.IsLocked = false;
                                mysqlClass.UpdateNewBuyTable(item);
                                bot.sendMessage($"{testmesaj}(Coin kilit çöz) {item.CoinName} kilit çözüldü.", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                return;
                            }
                        }
                    }
                }
                webOrder.state = WebOrderState.Close;
                mysqlClass.UpdateWebOrdersState(webOrder);
            }
            else if (webOrder.emir_kod == WebOrderCode.SetStopValue)
            {
                Debug.WriteLine("Coin stop value geldi.");
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                if (usersBudgets != null)
                {
                    foreach (var budget in usersBudgets)
                    {
                        Debug.WriteLine($"{budget} cüzdan bulundu.");
                        var emirler = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                        testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                        foreach (var item in emirler)
                        {
                            if (item.Id == webOrder.newbuytableid)
                            {
                                item.IsStop = true;
                                item.StopPercent = (double)webOrder.Parameter;
                                mysqlClass.UpdateNewBuyTable(item);
                                bot.sendMessage($"{testmesaj}(Coin set stop) {item.CoinName} için stop değeri ayarlandı.", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                return;
                            }
                        }
                    }
                }
                webOrder.state = WebOrderState.Close;
                mysqlClass.UpdateWebOrdersState(webOrder);
            }
            else if (webOrder.emir_kod == WebOrderCode.StopCancel)
            {
                Debug.WriteLine("Coin stop iptal geldi.");
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                if (usersBudgets != null)
                {
                    foreach (var budget in usersBudgets)
                    {
                        Debug.WriteLine($"{budget} cüzdan bulundu.");
                        var emirler = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                        testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                        foreach (var item in emirler)
                        {
                            if (item.Id == webOrder.newbuytableid)
                            {
                                item.IsStop = false;
                                item.StopPercent = 0;
                                mysqlClass.UpdateNewBuyTable(item);
                                bot.sendMessage($"{testmesaj}(Coin stop iptal) {item.CoinName} için stop değeri iptal edildi.", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                return;
                            }
                        }
                    }
                }
                webOrder.state = WebOrderState.Close;
                mysqlClass.UpdateWebOrdersState(webOrder);
            }
            else if (webOrder.emir_kod == WebOrderCode.DepositMoney)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Para yatırma geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        foreach (var budget in usersBudgets)
                        {
                            testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                            if (budget.Id == webOrder.newbuytableid)
                            {
                                Debug.WriteLine($"{budget} cüzdan bulundu.");
                                budget.DepositMoney += (double)webOrder.Parameter;
                                budget.RemainingMoney += (double)webOrder.Parameter;
                                mysqlClass.UpdateUserBadgetDepositAndWithdraw(budget);
                                bot.sendMessage($"{testmesaj}(Para ekle) {budget.BudgetName} için {webOrder.Parameter}$ eklendi.", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                return;
                            }
                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.WithdrawMoney)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Para çekme geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        foreach (var budget in usersBudgets)
                        {
                            testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                            if (budget.Id == webOrder.newbuytableid)
                            {
                                Debug.WriteLine($"{budget} cüzdan bulundu.");
                                budget.Withdrawmoney += (double)webOrder.Parameter;
                                budget.RemainingMoney -= (double)webOrder.Parameter;
                                mysqlClass.UpdateUserBadgetDepositAndWithdraw(budget);
                                bot.sendMessage($"{testmesaj}(Para çekme) {budget.BudgetName} için {webOrder.Parameter}$ çekildi.", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                return;
                            }
                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.PauseTrade)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Trade durdurma geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        foreach (var budget in usersBudgets)
                        {
                            testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                            if (budget.Id == webOrder.newbuytableid)
                            {
                                Debug.WriteLine($"{budget} cüzdan bulundu.");
                                budget.TradeEnable = 0;
                                mysqlClass.UpdateUserBadgetDepositAndWithdraw(budget);
                                bot.sendMessage($"{testmesaj}(Trade durdur) {budget.BudgetName} için trade durduruldu.", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                return;
                            }
                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.ContinueTrade)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Trade devam etme geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        foreach (var budget in usersBudgets)
                        {
                            testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                            if (budget.Id == webOrder.newbuytableid)
                            {
                                Debug.WriteLine($"{budget} cüzdan bulundu.");
                                budget.TradeEnable = 1;
                                mysqlClass.UpdateUserBadgetDepositAndWithdraw(budget);
                                bot.sendMessage($"{testmesaj}(Trade devam) {budget.BudgetName} için trade başlatıldı.", user.Groupid);
                                webOrder.state = WebOrderState.Close;
                                mysqlClass.UpdateWebOrdersState(webOrder);
                                return;
                            }
                        }
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.PauseAllTrade)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Tüm Trade durdurma geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        foreach (var budget in usersBudgets)
                        {
                                Debug.WriteLine($"{budget} cüzdan bulundu.");
                            testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                            budget.TradeEnable = 0;
                                mysqlClass.UpdateUserBadgetDepositAndWithdraw(budget);
                                bot.sendMessage($"{testmesaj}(Bütün Trade durdur) {budget.BudgetName} için trade durduruldu.", user.Groupid);
                        }
                        user.Tradeenable = false;
                        mysqlClass.UpdateUser(user);
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if (webOrder.emir_kod == WebOrderCode.ContinueAllTrade)
            {
                lock (lockobj)
                {
                    Debug.WriteLine("Tüm Trade başlatma geldi.");
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    if (usersBudgets != null)
                    {
                        foreach (var budget in usersBudgets)
                        {
                            testmesaj = budget.IsAlive == 0 ? "(Test)" : "";
                            Debug.WriteLine($"{budget} cüzdan bulundu.");
                            budget.TradeEnable = 1;
                            mysqlClass.UpdateUserBadgetDepositAndWithdraw(budget);
                            bot.sendMessage($"{testmesaj}(Bütün Trade başlat) {budget.BudgetName} için trade başlatıldı.", user.Groupid);
                        }
                        user.Tradeenable = true;
                        mysqlClass.UpdateUser(user);
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);
                }
            }
            else if(webOrder.emir_kod==WebOrderCode.AutobotEnable)
            {
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                foreach (var budget in usersBudgets)
                {
                    if (budget.Id == webOrder.newbuytableid)
                    {
                        budget.autobotenable = true;
                        budget.bottradeenable = true;
                        mysqlClass.UpdateUserBadgetAutobot(budget);
                    }
                }
                webOrder.state = WebOrderState.Close;
                mysqlClass.UpdateWebOrdersState(webOrder);
                bot.sendMessage($"Autobot kontrolü devreye alındı.", user.Groupid);
                AutobotControl();
            }
            else if (webOrder.emir_kod == WebOrderCode.AutobotDisable)
            {
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                foreach (var budget in usersBudgets)
                {
                    if (budget.Id == webOrder.newbuytableid)
                    {
                        budget.autobotenable = false;
                        budget.bottradeenable = true;
                        mysqlClass.UpdateUserBadgetAutobot(budget);
                    }
                }
                webOrder.state = WebOrderState.Close;
                mysqlClass.UpdateWebOrdersState(webOrder);
                bot.sendMessage($"Autobot kontolü kapatıldı.", user.Groupid);
            }
            else if(webOrder.emir_kod==WebOrderCode.SetStopLossDefault)
            {
                try
                {
                    var stoplossval = (double)webOrder.Parameter;
                    mysqlClass.UpdateStopLossValue(stoplossval);
                    bot.sendMessage($"Stop loss değeri {user.Userid}-{user.UserName} tarafından {stoplossval.ToString("F2")} olarak değiştirildi.", user.Groupid);
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);

                }
                catch (Exception)
                {
                    webOrder.state = WebOrderState.Error;
                    mysqlClass.UpdateWebOrdersState(webOrder);


                }
            }
            else if (webOrder.emir_kod == WebOrderCode.SetStopLossCheckDefault)
            {
                try
                {
                    var stoplosscheckval = (double)webOrder.Parameter;
                    mysqlClass.UpdateStopLossCheckValue(stoplosscheckval);
                    bot.sendMessage($"Stop loss kontrol değeri {user.Userid}-{user.UserName} tarafından {stoplosscheckval.ToString("F2")} olarak değiştirildi.", user.Groupid);
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);

                }
                catch (Exception)
                {
                    webOrder.state = WebOrderState.Error;
                    mysqlClass.UpdateWebOrdersState(webOrder);


                }
            }
            else if (webOrder.emir_kod == WebOrderCode.AddCoin)
            {
                try
                {
                    var coinname = webOrder.Parameter2;
                    var coins = mysqlClass.GetCoinList(false);
                    var bulunan = coins.Find(x => x.name == coinname);
                    bool eklendi = false;
                    if(bulunan==null)
                    {
                        mysqlClass.SaveCoin(coinname);
                        eklendi = true;
                    }
                    else
                    {
                        if(!bulunan.active)
                        {
                            bulunan.active = true;
                            mysqlClass.UpdateCoinlist(bulunan);
                            eklendi = true;
                        }
                    }
                    if (eklendi)
                    {
                        var price = binanceServices.GetTickerPrice(coinname);
                        try
                        {
                            socketConnection.CoinEkle(coinname, price[0].Price);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("addcoin error.\n" + ex.ToString());
                        }
                        bot.sendMessage($"{user.Userid}-{user.UserName} tarafından {coinname} eklendi.", user.Groupid);
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);

                }
                catch (Exception)
                {
                    webOrder.state = WebOrderState.Error;
                    mysqlClass.UpdateWebOrdersState(webOrder);


                }
            }
            else if (webOrder.emir_kod == WebOrderCode.DeleteCoin)
            {
                try
                {
                    var coinname = webOrder.Parameter2;
                    if (!mysqlClass.IsThereAnyOpenOrder(coinname))
                    {
                        var coins = mysqlClass.GetCoinList(false);
                        var bulunan = coins.Find(x => x.name == coinname);
                        if (bulunan != null)
                        {
                            if (bulunan.active)
                            {
                                bulunan.active = false;
                                mysqlClass.UpdateCoinlist(bulunan);
                                socketConnection.CoinSil(coinname);
                                bot.sendMessage($"{user.Userid}-{user.UserName} tarafından {coinname} silindi.", user.Groupid);
                            }
                        }
                    }
                    else
                    {
                        bot.sendMessage($"{user.Userid}-{user.UserName} tarafından {coinname} aktif trade olduğu için silinemedi.", user.Groupid);
                    }
                    webOrder.state = WebOrderState.Close;
                    mysqlClass.UpdateWebOrdersState(webOrder);

                }
                catch (Exception)
                {
                    webOrder.state = WebOrderState.Error;
                    mysqlClass.UpdateWebOrdersState(webOrder);


                }
            }
            else if (webOrder.emir_kod == WebOrderCode.SetBudgetStopLossEnable)
            {
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                var deger = (double)webOrder.Parameter;
                foreach (var budget in usersBudgets)
                {
                    if (budget.Id == webOrder.newbuytableid)
                    {
                        budget.stoplossenable = deger == 1 ? true : false;
                        mysqlClass.UpdateUserBadgetStopLoss(budget);
                    }
                }
                webOrder.state = WebOrderState.Close;
                mysqlClass.UpdateWebOrdersState(webOrder);
                if (deger == 1)
                    bot.sendMessage($"Cüzdan stop loss aktif edildi.", user.Groupid);
                else if (deger == 0)
                    bot.sendMessage($"Cüzdan stop loss pasif edildi.", user.Groupid);
            }
            else if (webOrder.emir_kod == WebOrderCode.SetBudgetStopLossValue)
            {
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                var deger = (double)webOrder.Parameter;
                foreach (var budget in usersBudgets)
                {
                    if (budget.Id == webOrder.newbuytableid)
                    {
                        if (deger <= 0)
                        {
                            budget.stoplossvalue = deger;
                            mysqlClass.UpdateUserBadgetStopLoss(budget);
                        }
                    }
                }
                webOrder.state = WebOrderState.Close;
                mysqlClass.UpdateWebOrdersState(webOrder);
                if (deger == 0)
                    bot.sendMessage($"Cüzdan stop loss sistem belirleyecek şeklinde değişti.", user.Groupid);
                else if (deger < 0)
                    bot.sendMessage($"Cüzdan stop loss değeri %{deger} olarak set edildi.", user.Groupid);
            }

        }

        public void AutobotControl()
        {
            if (GlobalVars.BotFullStop)
            {
                // kullanıcı bot a izin vermiş ve bot stop olmuş
                Debug.WriteLine("Autobot Sat emri geldi.");
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                if (usersBudgets != null)
                {
                    foreach (var budget in usersBudgets)
                    {
                        if (budget.BudgetName.StartsWith("xyz"))
                            continue;
                        if (budget.autobotenable && budget.bottradeenable)
                        {
                            Debug.WriteLine($"{budget.BudgetName} cüzdan bulundu.");
                            var emirler = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                            foreach (var emir in emirler)
                            {
                                Order order = binanceServices.CreateMarketSellOrder(emir.CoinName, emir.TotalAdet, budget, GetFiyat(emir.CoinName, budget));
                                if (order == null)
                                {
                                    bot.sendMessage($"Autobot {emir.CoinName} satışı için emir oluşturulamadı.", user.Groupid);
                                    continue;
                                }
                                emir.Sell(order.cummulativeQuoteQty / emir.TotalAdet, "Otobot", 0);
                                mysqlClass.UpdateNewBuyTable(emir);
                                if (emir.TotalAdet > 0)
                                    budget.CoinSelled(order.cummulativeQuoteQty);
                                mysqlClass.UpdateUserBadget(budget);
                            }
                            budget.bottradeenable = false;
                            mysqlClass.UpdateUserBadgetAutobot(budget);
                            bot.sendMessage($"Autobot kontrolü ile {budget.BudgetName} bot alım-satıma KAPATILDI.", user.Groupid);
                        }
                    }
                }

            }
            else if (!GlobalVars.BotFullStop)
            {
                Debug.WriteLine("Autobot start emri geldi.");
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                if (usersBudgets != null)
                {
                    foreach (var budget in usersBudgets)
                    {
                        if (budget.BudgetName.StartsWith("xyz"))
                            continue;
                        if (!budget.autobotenable)
                            continue;
                        if (budget.bottradeenable)
                            continue;
                        budget.bottradeenable = true;
                        if (budget.autobotenable && budget.TradeEnable==1)
                        {
                            bot.sendMessage($"Autobot kontrolü ile {budget.BudgetName} bot alım-satım başladı.", user.Groupid);
                        }
                        mysqlClass.UpdateUserBadgetAutobot(budget);
                    }
                }
            }
            if(GlobalVars.OnlyBuyStop && !GlobalVars.OncekiOnlyBuyStop && !GlobalVars.BotFullStop)
                bot.sendMessage($"Analize göre bot sadece alıma kapanma şartlarını sağladı. Bilginize.", user.Groupid);
            else if (!GlobalVars.OnlyBuyStop && GlobalVars.OncekiOnlyBuyStop && !GlobalVars.BotFullStop)
                bot.sendMessage($"Analize göre bot sadece alıma açılma şartlarını sağladı. Bilginize.", user.Groupid);
            if (GlobalVars.BotFullStop != GlobalVars.OncekiBotFullStop)
            {
                if(!GlobalVars.BotFullStop)
                    bot.sendMessage($"Analize göre bot START kriterlerini sağladı. Bilginize.", user.Groupid);
                else
                    bot.sendMessage($"!Analize göre bot STOP kriterlerini sağladı. Bilginize.", user.Groupid);
            }

        }
        public void SellCoins(long chatid,int cuzdanId,string coinname = "ALL" )
        {
            if (coinname == "ALL")
            {
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                foreach (var budget in usersBudgets)
                {
                    if (budget.TradeEnable == 0)
                        continue;
                    if (budget.Id != cuzdanId)
                        continue;
                    var buytables = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                    foreach (var alinmiscoin in buytables)
                    {
                        Order order = binanceServices.CreateMarketSellOrder(alinmiscoin.CoinName, alinmiscoin.TotalAdet, budget, GetFiyat(alinmiscoin.CoinName, budget));
                        if (order == null)
                            continue;
                        alinmiscoin.Sell(order.cummulativeQuoteQty / alinmiscoin.TotalAdet, "Kullanıcı", 0);
                        mysqlClass.UpdateNewBuyTable(alinmiscoin);
                        bot.sendMessage($"{alinmiscoin.CoinName} satıldı. Fiyat={(order.cummulativeQuoteQty / alinmiscoin.TotalAdet).ToString("F8")}-Adet={alinmiscoin.TotalAdet}-K/Z=%{alinmiscoin.SellProfit.ToString("F3")}", chatid);
                        if (alinmiscoin.TotalAdet > 0)
                            budget.CoinSelled(order.cummulativeQuoteQty);
                    }
                    mysqlClass.UpdateUserBadget(budget);
                }
            }
            else
            {
                usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                foreach (var budget in usersBudgets)
                {
                    if (budget.TradeEnable == 0)
                        continue;
                    if (budget.Id != cuzdanId)
                        continue;
                    var buytables = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                    var bulunanlar = buytables.FindAll(x => x.CoinName == coinname);
                    if(bulunanlar.Count==0)
                    {
                        bot.sendMessage($"{coinname} isimli coin listenizde bulunamadı.", chatid);
                        return;
                    }
                    foreach (var alinmiscoin in bulunanlar)
                    {
                        Order order = binanceServices.CreateMarketSellOrder(alinmiscoin.CoinName, alinmiscoin.TotalAdet,budget, GetFiyat(alinmiscoin.CoinName, budget));
                        if (order == null)
                            continue;
                        alinmiscoin.Sell(order.cummulativeQuoteQty / alinmiscoin.TotalAdet, "Kullanıcı", 0);
                        mysqlClass.UpdateNewBuyTable(alinmiscoin);
                        bot.sendMessage($"{alinmiscoin.CoinName} satıldı. Fiyat={(order.cummulativeQuoteQty / alinmiscoin.TotalAdet).ToString("F8")}-Adet={alinmiscoin.TotalAdet}-K/Z=%{alinmiscoin.SellProfit.ToString("F3")}", chatid);
                        if (alinmiscoin.TotalAdet > 0)
                            budget.CoinSelled(order.cummulativeQuoteQty);
                    }
                    mysqlClass.UpdateUserBadget(budget);
                }
            }
        }
        public void DoBuyOrSell(List<NewBuyTable> alimlist, List<NewBuyTable> satimlist, string sure)
        {
            lock (lockobj)
            {

                
                if (user.Tradeenable)
                {
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    Debug.WriteLine($"{user.UserName} buy");
                    List<NewBuyTable> buytables;
                    foreach (var budget in usersBudgets)
                    {
                        if (budget.BudgetName.EndsWith("_1G") && sure == "4h")
                            continue;
                        if (!budget.BudgetName.EndsWith("_1G") && sure == "1d")
                            continue;
                        if (budget.TradeEnable == 0 || !budget.bottradeenable)
                            continue;
                        // bekleyen listesinde var ise silelim
                        var bekleyenler = mysqlClass.GetNewBuyTables(States.Waiting, budget.BudgetName);
                        foreach (var item in bekleyenler)
                        {
                            var satimdavarmi = satimlist.Find(x => x.CoinName == item.CoinName);
                            if (satimdavarmi == null)
                                continue;
                            item.State = States.Canceled;
                            mysqlClass.UpdateOnlyState(item);
                        }

                        // önce satılacakları satalım
                        buytables = mysqlClass.GetNewBuyTables(States.Open, budget.BudgetName);
                        foreach (var alinmiscoin in buytables)
                        {
                            var coinbilgileri = satimlist.Find(x => x.CoinName == alinmiscoin.CoinName);
                            if (coinbilgileri == null)
                                continue;

                            Debug.WriteLine($"{user.UserName}-{budget.BudgetName} marketsell1");
                            if (!CoinIsOk(alinmiscoin.CoinName, alinmiscoin.TotalAdet,budget))
                            {
                                if (alinmiscoin.SellPrice != -1)
                                {
                                    bot.sendMessage($"{alinmiscoin.CoinName} satışı için elinizde yeterli adet yok. Düzeltiniz.", user.Groupid);
                                    alinmiscoin.SellPrice = -1;
                                    mysqlClass.UpdateSellPrice(alinmiscoin);
                                    continue;
                                }
                                continue;
                            }
                            Order order = binanceServices.CreateMarketSellOrder(alinmiscoin.CoinName, alinmiscoin.TotalAdet, budget, GetFiyat(alinmiscoin.CoinName, budget));
                            if (order == null)
                                continue;
                            alinmiscoin.Sell(order.cummulativeQuoteQty / alinmiscoin.TotalAdet, "MTS", coinbilgileri.AlisMTS);
                            mysqlClass.UpdateNewBuyTable(alinmiscoin);
                            string testmessage = budget.IsAlive == 0 ? "(TEST)" : "";
                            bot.sendMessage($"{testmessage}{alinmiscoin.CoinName} satıldı. Fiyat={(order.cummulativeQuoteQty / alinmiscoin.TotalAdet).ToString("F8")}-Adet={alinmiscoin.TotalAdet}-K/Z=%{alinmiscoin.SellProfit.ToString("F3")}", user.Groupid);
                            if (alinmiscoin.TotalAdet > 0)
                            {
                                budget.CoinSelled(order.cummulativeQuoteQty);
                                mysqlClass.UpdateUserBadgetTradeAndMoney(budget);
                            }

                        }
                    }

                    foreach (var budget in usersBudgets)
                    {
                        if (budget.BudgetName.EndsWith("_1G") && sure == "4h")
                            continue;
                        if (!budget.BudgetName.EndsWith("_1G") && sure == "1d")
                            continue;

                        // state=3 olanlar burada silinecek
                        //var bekleyenler = mysqlClass.GetNewBuyTables(States.Waiting, budget.BudgetName);
                        //foreach (var item in bekleyenler)
                        //{
                        //    item.State = States.Canceled;
                        //    mysqlClass.UpdateOnlyState(item);
                        //}
                        var yenialimlist = alimlist.OrderBy(x => x.CoinName).ToList();
                        foreach (var alinacak in yenialimlist)
                        {
                            Debug.WriteLine($"{budget.BudgetName} için bekleyen {alinacak.CoinName}. Limit {alinacak.BuyLimit}");
                            var sonfiyat = socketConnection.GetPrice(alinacak.CoinName);
                            if (sonfiyat == 0)
                                continue;
                            sonfiyat = sonfiyat * 0.999;
                            var newbuytabletemp = new NewBuyTable()
                            {
                                CoinName = alinacak.CoinName,
                                BuyDate = DateHelper.GetCurrentTimeStam(),
                                BuyPrice = sonfiyat,
                                TotalAdet = 0,
                                TotalMoney = 0,
                                ProfitNow = 0,
                                AlisMTS = alinacak.AlisMTS,
                                State = States.Waiting,
                                UserName = budget.BudgetName,
                                SellReason = "",
                            };
                            newbuytabletemp.IsStop = false;
                            newbuytabletemp.PriceNow = sonfiyat;
                            newbuytabletemp.BuyDatestr = DateHelper.GetDateStrFromTimeStamp(newbuytabletemp.BuyDate);
                            newbuytabletemp.trackstop = -1;
                            mysqlClass.SaveNewBuyTable(newbuytabletemp, budget.BudgetName);
                        }

                    }
                }
            }
        }
        private bool coinalmethod(UsersBudget budget, NewBuyTable newBuyTable)
        {
            

            List<NewBuyTable> yenialimlist = new List<NewBuyTable>() { newBuyTable};

            //Debug.WriteLine($"{yenialimlist.Count} adet var listede.");

            if (budget.TradeNow >= budget.TradeMax || budget.RemainingMoney < 0)
            {
                //Debug.WriteLine($"{user.UserName}-{budget.BudgetName} Trade veya para yetersiz. Çıkıyorum.");
                //mysqlClass.UpdateUserBadget(budget);
                return false;
            }
            Debug.WriteLine("Balance okunuyor.");

            double balances = budget.IsAlive==0 ? double.MaxValue : binanceServices.GetBalanceFree("BUSD"); //XXX USDT
            Debug.WriteLine("BUSD okundu");
            double balancesbnb = budget.IsAlive == 0 ? double.MaxValue : binanceServices.GetBalanceFree("BNB");

            var fiyatlar = socketConnection.GetPrice("BNBUSDT");    //prices.Find(x=>x.Symbol=="BNBUSDT");
            double balancebnbUSDT = balancesbnb * (fiyatlar);// != null ? fiyatlar.Price : 0);
            Debug.WriteLine($"Önce->Balance: {balances} BalanceBnb: {balancebnbUSDT}");

            double alimbutce = 0;

            if (budget.TradeTytpe == TradeType.ParaninYarisi)
            {
                if ((budget.TradeMax - budget.TradeNow) > 0)
                {
                    alimbutce = (budget.RemainingMoney / 2);
                    if (alimbutce <= 20)
                        alimbutce = 20;
                }
            }
            else if (budget.TradeTytpe == TradeType.EsitButce)
            {
                if ((budget.TradeMax - budget.TradeNow) > 0)
                {
                    alimbutce = (budget.RemainingMoney / (budget.TradeMax - budget.TradeNow));
                    if (alimbutce <= 20)
                        alimbutce = 20;
                }
            }
            Debug.WriteLine($"Alım bütçe: {alimbutce}");
            if (budget.IsAlive == 0)
            {
                balances = budget.RemainingMoney;
                balancebnbUSDT = int.MaxValue;
            }
            Debug.WriteLine($"Balance: {balances} BalanceBnb: {balancebnbUSDT}");
            string testmessage = budget.IsAlive == 0 ? "(TEST)" : ""; 
            if (balances < alimbutce)
            {
                if (budget.IsAlive == 1)
                {
                    bot.sendMessage($"{testmessage}Cüzdanınızda yeterli para kalmadı. Mevcut={balances.ToString("F3")}-İhtiyaç={alimbutce.ToString("F3")}", user.Groupid);
                }
                //mysqlClass.UpdateUserBadget(budget);
                return false;
            }
            // XXX iptal edildi
            //if (alimbutce * (0.1 / 100) > balancebnbUSDT)
            //{
            //    if (budget.IsAlive == 1)
            //    {
            //        bot.sendMessage($"{testmessage}Cüzdanınızda komisyon için yeterli BNB kalmadı. Mevcut={balancebnbUSDT}-İhtiyaç={alimbutce * (0.1 / 100)}", user.Groupid);
            //    }
            //    //mysqlClass.UpdateUserBadget(budget);
            //    return false;
            //}
            Debug.WriteLine($"Alıma girdim.");

            foreach (var alinacak in yenialimlist)
            {
 
                 Debug.WriteLine($"{user.UserName} için {alinacak.CoinName}-{alimbutce}");
                double fiyati = 0;
                if (budget.IsAlive==0)
                    fiyati = socketConnection.GetPrice(alinacak.CoinName);
                var order = binanceServices.CreateMarketBuyOrder(alinacak.CoinName, alimbutce, budget, fiyati);
                if (order == null)
                    continue;
                var newbuytable = new NewBuyTable()
                {
                    CoinName = alinacak.CoinName,
                    BuyDate = DateHelper.GetCurrentTimeStam(),
                    BuyPrice = order.cummulativeQuoteQty / order.executedQty,
                    TotalAdet = order.executedQty,
                    TotalMoney = order.cummulativeQuoteQty,
                    ProfitNow = 0,
                    AlisMTS = alinacak.AlisMTS,
                    State = States.Open,
                    UserName = budget.BudgetName,
                    SellReason =GlobalVars.CandleCount.ToString(),
                };
                //if (budget.BudgetName.StartsWith("xyz"))
                //  newbuytable.AlisMTS = alinacak.AlisMTS;
                newbuytable.IsStop = true;
                newbuytable.StopPercent = -5;
                newbuytable.PriceNow = newbuytable.BuyPrice;
                newbuytable.BuyDatestr = DateHelper.GetDateStrFromTimeStamp(newbuytable.BuyDate);
                var stoplossdefault = mysqlClass.GetStopLossValue();
                newbuytable.trackstop = -1;
                mysqlClass.SaveNewBuyTable(newbuytable, budget.BudgetName);
                budget.CoinBuyed(order.cummulativeQuoteQty);
                bot.sendMessage($"{testmessage}{budget.BudgetName}-{alinacak.CoinName} alindi. Fiyat={newbuytable.BuyPrice.ToString("F8")}-Adet={newbuytable.TotalAdet}", user.Groupid);
                mysqlClass.UpdateUserBadgetTradeAndMoney(budget);
                return true;
            }
            return false;
        }
    


            public void SetButce(int tradesayi, string butcetype, long chatid, int cuzdanId)
        {
            usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
            foreach (var budget in usersBudgets)
            {
                if (budget.Id != cuzdanId)
                    continue;
                budget.TradeMax = tradesayi;
                budget.TradeTytpe = butcetype == "ESIT" ? TradeType.EsitButce : TradeType.ParaninYarisi;
                mysqlClass.UpdateUserBadgetTrade(budget);
                bot.sendMessage($"{budget.BudgetName} için bütçe ayarlandı.", chatid);
            }
        }
        public void ButceEkle(int eklenecek, long chatid, int cuzdanId)
        {
            usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
            foreach (var budget in usersBudgets)
            {
                if (budget.Id != cuzdanId)
                    continue;
                budget.StartBudget+= eklenecek;
                budget.RemainingMoney += eklenecek;
                mysqlClass.UpdateUserBadgetButce(budget);
                bot.sendMessage($"{budget.BudgetName} için {eklenecek}$ eklendi.", chatid);
            }

        }
        public void ButceSil(int silinecek, long chatid, int cuzdanId)
        {
            usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
            foreach (var budget in usersBudgets)
            {
                if (budget.Id != cuzdanId)
                    continue;
                if (budget.RemainingMoney > silinecek)
                {
                    budget.RemainingMoney -= silinecek;
                    mysqlClass.UpdateUserBadgetButce(budget);
                    bot.sendMessage($"{budget.BudgetName} için {silinecek}$ silindi.", chatid);
                }
                else
                    bot.sendMessage($"{budget.BudgetName} için {silinecek}$ silmek için kalan paranız yok.", chatid);
            }

        }

        public bool CoinIsOk(string coinname, double miktar, UsersBudget budget)
        {
            if (budget.IsAlive == 0)
                return true;
            coinname = coinname.Substring(0, coinname.Length - 4);
            double balance = binanceServices.GetBalanceFree(coinname);
            if (balance >= miktar)
                return true;
            else
                return false;
        }
        public void GetBnbFree(double bnbfiyat)
        {
            if (GlobalVars.Istest)
                return;
            double bnbbalance = binanceServices.GetBalanceFree("BNB");
            //var sonfiyatlar = binanceServices.GetTickerPrice("BNBUSDT");
            //double bnbfiyat = 0;
            //if (sonfiyatlar != null)
            //{
            //    bnbfiyat = sonfiyatlar.First(x => x.Symbol == "BNBUSDT").Price;
            //}
            double bnbusdt = bnbbalance * bnbfiyat;
            var userbudgetsler = mysqlClass.GetUsersBudgets(user.UserName);
            foreach (var budget in userbudgetsler)
            {
                budget.bnbbalance = bnbbalance;
                budget.bnbbalanceusdt = bnbusdt;
                mysqlClass.UpdateUserBadgetBalance(budget);
            }

        }

        public void TargetReached(bool systemStatusNormal)
        {
            try
            {
                lock (lockobj)
                {
                    
                    usersBudgets = mysqlClass.GetUsersBudgets(user.UserName);
                    AtYarisiBudget = new List<UsersBudget>();
                    foreach (var budget in usersBudgets)
                    {
                        string testmessage =budget.IsAlive==0  ? "(TEST)" : "";
                        var buytables = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                        double alimdakilerinfiyatlari = 0.0;
                        foreach (var buytable in buytables)
                        {
                            double sonfiyat = socketConnection.GetPrice(buytable.CoinName);    //sonfiyatlar.First(x => x.Symbol == buytable.CoinName).Price;

                            buytable.PriceNow = sonfiyat;
                            alimdakilerinfiyatlari += buytable.PriceNow * buytable.TotalAdet;
                            buytable.ProfitCalc();
                            bool satalimmi = false;
                            string sebebim = "";
                            if (buytable.trackstop != -1 && (buytable.BuyPrice + (buytable.BuyPrice * buytable.trackstop) / 100) > sonfiyat)
                            {
                                satalimmi = true;
                                sebebim = $"Kar SL %{buytable.trackstop}";
                                //trackstopsatis = true;
                            }
                            if (!satalimmi && buytable.ProfitNow >= 2.5 && buytable.trackstop != -1)
                            {
                                buytable.trackstop = buytable.MaxProfit - 0.5;
                                mysqlClass.UpdateNewBuyTableTrackStop(buytable);
                            } 
                            if (!satalimmi && buytable.ProfitNow >= 2.5 && buytable.trackstop == -1)
                            {
                                buytable.trackstop = buytable.MaxProfit-0.5;
                                mysqlClass.UpdateNewBuyTableTrackStop(buytable);
                            }

                            if (satalimmi)
                            {
                                if (!CoinIsOk(buytable.CoinName, buytable.TotalAdet, budget))
                                {
                                    if (buytable.SellPrice != -1)
                                    {
                                        bot.sendMessage($"{buytable.CoinName} satışı için elinizde yeterli adet yok. Düzeltiniz.", user.Groupid);
                                        buytable.SellPrice = -1;
                                        mysqlClass.UpdateSellPrice(buytable);
                                        continue;
                                    }
                                    continue;
                                }
                                Order order = binanceServices.CreateMarketSellOrder(buytable.CoinName, buytable.TotalAdet, budget, GetFiyat(buytable.CoinName, budget));
                                if (order == null)
                                    continue;
                                buytable.Sell(order.cummulativeQuoteQty / buytable.TotalAdet, sebebim, buytable.SellCandleMTS);
                                mysqlClass.UpdateNewBuyTable(buytable);
                                bot.sendMessage($"{testmessage}{buytable.CoinName} satıldı. Fiyat={(order.cummulativeQuoteQty / buytable.TotalAdet).ToString("F8")}-Adet={buytable.TotalAdet}-K/Z=%{buytable.SellProfit.ToString("F3")}", user.Groupid);
                                if (buytable.TotalAdet > 0)
                                {
                                    budget.CoinSelled(order.cummulativeQuoteQty);
                                    mysqlClass.UpdateUserBadgetTradeAndMoney(budget);
                                }
                                continue;
                            }
                        }
                        buytables = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                        alimdakilerinfiyatlari = 0.0;
                        foreach (var buytable in buytables)
                        {
                            double sonfiyat = socketConnection.GetPrice(buytable.CoinName);    //sonfiyatlar.First(x => x.Symbol == buytable.CoinName).Price;

                            buytable.PriceNow = sonfiyat;
                            alimdakilerinfiyatlari += buytable.PriceNow * buytable.TotalAdet;
                            buytable.ProfitCalc();
                        }
                        
                        budget.CalculateProfits(alimdakilerinfiyatlari);
                        bool satalim = false;
                        string sebep = "";

                        if (budget.realprofit >= 2.5) 
                        {
                            satalim = true;
                            sebep = "%2.5 kazanç";
                            if (budget.TradeMax == 1 && budget.TradeNow == 1)
                            {
                                buytables = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                                foreach (var item in buytables)
                                {
                                    if (item.trackstop == -1)
                                        satalim = true;
                                    else
                                    {
                                        satalim = false;
                                        sebep = "";
                                    }
                                }
                            }

                        }
                        else if(budget.realprofit<=-2.5)
                        {
                            satalim = true;
                            sebep = "%-2.5 zarar";
                        }
                        if (satalim)
                        {
                            double yenibaslangic = budget.RemainingMoney;
                            bool hatavar = false;
                            foreach (var buytable in buytables)
                            {
                                Order order = binanceServices.CreateMarketSellOrder(buytable.CoinName, buytable.TotalAdet, budget, GetFiyat(buytable.CoinName, budget));
                                if (order == null)
                                {
                                    hatavar = true;
                                    bot.sendMessage($"{testmessage}{budget.BudgetName}-{buytable.CoinName} satılamadı.", user.Groupid);
                                    continue;
                                }
                                yenibaslangic += order.cummulativeQuoteQty;
                                buytable.Sell(order.cummulativeQuoteQty / buytable.TotalAdet, sebep, buytable.SellCandleMTS);
                                mysqlClass.UpdateNewBuyTable(buytable);
                                bot.sendMessage($"{testmessage}{budget.BudgetName}-{buytable.CoinName} satıldı. Fiyat={(order.cummulativeQuoteQty / buytable.TotalAdet).ToString("F8")}-Adet={buytable.TotalAdet}-K/Z=%{buytable.SellProfit.ToString("F3")}", user.Groupid);
                                if (buytable.TotalAdet > 0)
                                {
                                    budget.CoinSelled(order.cummulativeQuoteQty);
                                    mysqlClass.UpdateUserBadgetTradeAndMoney(budget);
                                }
                            }
                            if(!hatavar)
                            {
                                budget.StartBudget = yenibaslangic;
                                budget.TargetBudget = 0;
                                mysqlClass.UpdateUserBadget(budget);
                                bot.sendMessage($"{testmessage}{budget.BudgetName} için yeni başlangıç= {budget.StartBudget.ToString("F3")}", user.Groupid);
                            }
                        }
                            /*
                            //if (!satalim && buytable.trackstop != -1 && (buytable.BuyPrice + (buytable.BuyPrice * buytable.trackstop) / 100) > sonfiyat)
                            //{
                            //    satalim = true;
                            //    sebep = $"Kar SL %{buytable.trackstop}";
                            //    //trackstopsatis = true;
                            //}
                            //if(!satalim && buytable.trackstop != -1)// && (buytable.BuyPrice + (buytable.BuyPrice * (buytable.trackstop+4.0) / 100) <= sonfiyat))
                            //{
                            //    if(!budget.BudgetName.EndsWith("_1G"))
                            //        buytable.trackstop = buytable.MaxProfit - 2;
                            //    else
                            //        buytable.trackstop = buytable.MaxProfit - 5;
                            //    //double artis = 2;
                            //    //double basl = 0;
                            //    //for(basl=buytable.trackstop;basl<500;basl+=artis)
                            //    //{
                            //    //    if (buytable.BuyPrice + (buytable.BuyPrice * (basl + artis) / 100) <= sonfiyat)
                            //    //        continue;
                            //    //    else
                            //    //        break;
                            //    //}
                            //    //buytable.trackstop = basl - artis;
                            //    mysqlClass.UpdateNewBuyTableTrackStop(buytable);
                            //    mysqlClass.UpdateNewBuyTableExceptState(buytable);
                            //    continue;

                            //}
                            //if (buytable.ProfitNow >= 2 && buytable.trackstop==-1 && !budget.BudgetName.EndsWith("_1G"))
                            //{
                            //    //satalim = true;
                            //    //sebep = "%2.5>";
                            //    buytable.trackstop = 0;
                            //    mysqlClass.UpdateNewBuyTableTrackStop(buytable);
                            //    continue;
                            //}
                            //if (buytable.ProfitNow >= 5 && buytable.trackstop == -1 && budget.BudgetName.EndsWith("_1G"))
                            //{
                            //    //satalim = true;
                            //    //sebep = "%2.5>";
                            //    buytable.trackstop = 0;
                            //    mysqlClass.UpdateNewBuyTableTrackStop(buytable);
                            //    continue;
                            //}

                            //if (!satalim && buytable.IsStop && (buytable.BuyPrice * (100 + buytable.StopPercent) / 100) > sonfiyat)
                            //{
                            //    var bannedlist = mysqlClass.GetBannedList();
                            //    satalim = true;
                            //    sebep = $"Stop loss %{buytable.StopPercent}";
                            //    if (!bannedlist.Contains(buytable.CoinName))
                            //        mysqlClass.SaveBanlist(buytable.CoinName);
                            //}*/
                            

                        var buytablesbekleyen = mysqlClass.GetNewBuyTables(States.Waiting, budget.BudgetName);
                        foreach (var buytable in buytablesbekleyen)
                        {
                            double sonfiyat = socketConnection.GetPrice(buytable.CoinName);    //sonfiyatlar.First(x => x.Symbol == buytable.CoinName).Price;

                            buytable.PriceNow = sonfiyat;
                            buytable.ProfitNow = buytable.CalculateProfitNow(sonfiyat);
                            //if(buytable.PriceNow>buytable.BuyPrice)
                            {
                                mysqlClass.UpdateOnlyState(buytable);
                                //continue;
                            }
                        }
                        foreach (var buytable in buytablesbekleyen.OrderBy(x=>x.ProfitNow).ToList())
                        {
                            if (buytable.ProfitNow <= 0)
                            {
                                if (budget.TradeNow != budget.TradeMax)
                                {
                                    var sonuc = coinalmethod(budget, buytable);
                                    if (sonuc)
                                        buytable.State = States.WaitingBuyed;
                                    mysqlClass.UpdateOnlyState(buytable);
                                }
                            }
                        }

                        buytables = mysqlClass.GetNewBuyTables(1, budget.BudgetName);
                        alimdakilerinfiyatlari = 0.0;
                        foreach (var buytable in buytables)
                        {
                            double sonfiyat = socketConnection.GetPrice(buytable.CoinName);    //sonfiyatlar.First(x => x.Symbol == buytable.CoinName).Price;

                            buytable.PriceNow = sonfiyat;
                            alimdakilerinfiyatlari += buytable.PriceNow * buytable.TotalAdet;
                            buytable.ProfitCalc();
                            mysqlClass.UpdateNewBuyTableExceptState(buytable);
                        }

                        //mysqlClass.UpdateUserBadget(budget);
                        var aktifler = mysqlClass.GetNewBuyTables(States.Open, budget.BudgetName).OrderByDescending(x => x.BuyDate).ToList();
                        var pasifler = mysqlClass.GetNewBuyTables(States.Sold, budget.BudgetName).OrderByDescending(x => x.SellDate).ToList();
                        var bekleyenler= mysqlClass.GetNewBuyTables(States.Waiting, budget.BudgetName).OrderByDescending(x => x.ProfitNow).ToList();
                        double gercekusdt = budget.RemainingMoney + alimdakilerinfiyatlari;

                        budget.CalculateProfits(alimdakilerinfiyatlari);
                        mysqlClass.UpdateUserBadgetWebView(budget);

                        budget.StartBudget = budget.StartBudget + budget.DepositMoney - budget.Withdrawmoney;
                        if (budget.StartBudget == 0)
                            budget.StartBudget = 0.1;
                        HtmlHelper.GetDipBuyTableHtmlTable(aktifler, pasifler, $"{user.UserName}_{budget.BudgetName}_{budget.HtmlHeader}", budget, gercekusdt,bekleyenler);
                        AtYarisiBudget.Add(new UsersBudget()
                        {
                            RemainingMoney = gercekusdt,
                            HtmlHeader = $"http://{HtmlHelper.PublicIp}/{user.UserName}_{budget.BudgetName}_{budget.HtmlHeader}.htm",
                            UserName = $"{user.UserName}_{budget.BudgetName}_{budget.HtmlHeader}",
                            NetWorth = (((gercekusdt - budget.TargetBudget) / budget.StartBudget) - 1) * 100,
                            TradeEnable=budget.TradeEnable
                        });
                        if (DateTime.Now.Hour == 8 && DateTime.Now.Minute == 0)
                        {
                            var hepsi = mysqlClass.GetNewBuyTables(States.All, budget.BudgetName).OrderBy(x => x.CoinName).ThenBy(x => x.BuyDate).ToList();
                            HtmlHelper.GetDipBuyTableHtmlTable(new List<NewBuyTable>(), hepsi, $"{user.UserName}_{budget.BudgetName}_{budget.HtmlHeader}_hepsi", budget, gercekusdt,new List<NewBuyTable>());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }   
        }

        public void PNLhesap()
        {
            lock (pnllockobj)
            {
                if(!systemStatusNormal)
                {
                    bot.sendMessage("Sistem bakımda olduğu için PNL gönderilemiyor.", user.Groupid);
                    return;
                }
                AccountInfo info = binanceServices.GetAccountAsync();

                List<Prices> prices = new List<Prices>();
                prices = binanceServices.ListPrices();

                double total;
                double anaPNL = 0;
                double anaPNLBTC = 0;
                double anaPNLTRY = 0;
                bool atla = false;
                double cuzdanhesap = 0;
                Prices price;
                string sonfiyatasset;
                double fiyat;
                double btcfiyat = 0;
                double busdfiyat = 0;
                btcfiyat = prices.FirstOrDefault(pricem => pricem.Symbol == "BTCUSDT").Price;
                busdfiyat = prices.FirstOrDefault(pricem => pricem.Symbol == "BUSDUSDT").Price;
                foreach (var item in info.balances)
                {
                    total = item.free + item.locked;
                    atla = false;
                    if (total > 0)
                    {
                        if (item.asset == "USDT")
                        {
                            anaPNL += total;
                            atla = true;
                        }
                        if (!atla)
                        {
                            sonfiyatasset = item.asset + "USDT";
                            price = prices.FirstOrDefault(pricem => pricem.Symbol == sonfiyatasset);
                            fiyat = price == null ? 0 : price.Price;
                            cuzdanhesap = fiyat * total;
                            if (fiyat == 0)
                            {
                                sonfiyatasset = item.asset + "BTC";
                                price = prices.FirstOrDefault(pricem => pricem.Symbol == sonfiyatasset);
                                fiyat = price == null ? 0 : price.Price;
                                double btcadet = fiyat * total;

                                cuzdanhesap = btcfiyat * btcadet;
                            }
                            if (fiyat == 0)
                            {
                                sonfiyatasset = item.asset + "BUSD";
                                price = prices.FirstOrDefault(pricem => pricem.Symbol == sonfiyatasset);
                                fiyat = price == null ? 0 : price.Price;
                                double busdadet = fiyat * total;

                                cuzdanhesap = busdfiyat * busdadet;
                            }
                            anaPNL += cuzdanhesap;
                        }
                        if (item.asset == "BTC")
                        {
                            anaPNLBTC += total;
                            continue;
                        }
                        if (item.asset == "USDT")
                            sonfiyatasset = "BTCUSDT";
                        else if (item.asset == "BUSD")
                            sonfiyatasset = "BTCBUSD";
                        else
                            sonfiyatasset = item.asset + "BTC";
                        price = prices.FirstOrDefault(pricem => pricem.Symbol == sonfiyatasset);
                        fiyat = price == null ? 0 : price.Price;
                        if (fiyat == 0)
                        {
                            if (item.asset.EndsWith("DOWN") || item.asset.EndsWith("UP"))
                            {
                                sonfiyatasset = item.asset + "USDT";
                                price = prices.FirstOrDefault(pricem => pricem.Symbol == sonfiyatasset);
                                fiyat = price == null ? 0 : price.Price;
                                double dolarkarsiligi = total * fiyat;
                                cuzdanhesap = dolarkarsiligi / btcfiyat;
                                continue;
                            }
                        }
                        if (item.asset == "USDT" || item.asset == "BUSD")
                            cuzdanhesap = total / fiyat;
                        else
                            cuzdanhesap = fiyat * total;
                        anaPNLBTC += cuzdanhesap;
                    }
                }
                price = prices.FirstOrDefault(pricem => pricem.Symbol == "USDTTRY");
                fiyat = price == null ? 0 : price.Price;
                anaPNLTRY = anaPNL * fiyat;
                var sonuc = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);       // DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var unix = ((DateTimeOffset)sonuc).ToUnixTimeMilliseconds();
                TimeSpan time = TimeSpan.FromMilliseconds(unix);
                DateTime startdate = new DateTime(1970, 1, 1) + time;
                var datestr = startdate.ToLocalTime().ToString();

                PNL mypnl = new PNL
                {
                    Dolar = anaPNL,
                    Btc = anaPNLBTC,
                    TRY = anaPNLTRY,
                    Tarih = unix,
                    Name = user.UserName,
                    Tarihstr = datestr
                };

                PNL openPNL = mysqlClass.GetOpenPNL(user.UserName);
                Percents percents = new Percents();
                if (openPNL != null)
                    percents = mypnl.CalculatePercent(openPNL);
                string message = DateTime.Now.ToString() + "\n";
                message += String.Format("Toplam {0:F3} $ (%{1:F2})", mypnl.Dolar, percents.DolarPercent) + "\n";
                message += String.Format("Toplam {0:F8} BTC (%{1:F2})", mypnl.Btc, percents.BtcPercent) + "\n";
                message += String.Format("Toplam {0:F3} TRY (%{1:F2})", mypnl.TRY, percents.TRYPercent);
                var task = bot.sendMessage(message, user.Groupid);
                Thread.Sleep(10000);
                if (user.UserName == "emrah")
                {
                    var xtzfiyat = prices.FirstOrDefault(pricem => pricem.Symbol == "XTZUSDT");
                    var xrpfiyat = prices.FirstOrDefault(pricem => pricem.Symbol == "XRPUSDT");
                    var bnbfiyat = prices.FirstOrDefault(pricem => pricem.Symbol == "BNBUSDT");
                    var ltcfiyat = prices.FirstOrDefault(pricem => pricem.Symbol == "LTCUSDT");
                    var adafiyat = prices.FirstOrDefault(pricem => pricem.Symbol == "ADAUSDT");
                    var trxfiyat = prices.FirstOrDefault(pricem => pricem.Symbol == "TRXUSDT");
                    var bttfiyat = prices.FirstOrDefault(pricem => pricem.Symbol == "BTTCUSDT");
                    double xtz = xtzfiyat == null ? 0 : xtzfiyat.Price * 203.74;
                    double xrp = xrpfiyat == null ? 0 : xrpfiyat.Price * 1741;
                    double bnb = bnbfiyat == null ? 0 : bnbfiyat.Price * 2.0077;
                    double ltc = ltcfiyat == null ? 0 : ltcfiyat.Price * 13.1;
                    double ada = adafiyat == null ? 0 : adafiyat.Price * 983.933061;
                    double trx = trxfiyat == null ? 0 : trxfiyat.Price * 1471.7;
                    double btt = bttfiyat == null ? 0 : bttfiyat.Price * 12099000.001;

                    double xtzX = xtzfiyat == null ? 0 : xtzfiyat.Price / 2.7922;
                    double xrpX = xrpfiyat == null ? 0 : xrpfiyat.Price / 0.7537;
                    double bnbX = bnbfiyat == null ? 0 : bnbfiyat.Price / 190.85;
                    double ltcX = ltcfiyat == null ? 0 : ltcfiyat.Price / 158.2156;
                    double adaX = adafiyat == null ? 0 : adafiyat.Price / 1.509;
                    double trxX = trxfiyat == null ? 0 : trxfiyat.Price / 0.10110;
                    double bttX = bttfiyat == null ? 0 : bttfiyat.Price / 0.000004716;

                    double totaluv = xtz + xrp + bnb + ltc + ada + trx + btt;
                    price = prices.FirstOrDefault(pricem => pricem.Symbol == "USDTTRY");
                    double totaluvtry = price == null ? 0 : totaluv * price.Price;
                    message = "UV listem:\n";
                    message += $"USDT : {totaluv.ToString("F2")}" + "\n";
                    message += $"TRY  : {totaluvtry.ToString("F2")}" + "\n";
                    message += $"XTZ->{xtzX.ToString("F2")}--2.79\n";
                    message += $"XRP->{xrpX.ToString("F2")}--0.75\n";
                    message += $"BNB->{bnbX.ToString("F2")}--190.85\n";
                    message += $"LTC->{ltcX.ToString("F2")}--156.22\n";
                    message += $"ADA->{adaX.ToString("F2")}--1.50\n";
                    message += $"TRX->{trxX.ToString("F2")}--0.10110\n";
                    message += $"BTT->{bttX.ToString("F2")}--0.004716";
                    task = bot.sendMessage(message, user.Groupid);
                }
                if (DateTime.Now.Hour == 3)
                {
                    mysqlClass.SavePNL(mypnl);
                }
            }
        }
    }

}
  
