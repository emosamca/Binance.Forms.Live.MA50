using Binance.Commons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Binance.Connections
{
    public class Services
    {
        private readonly Client _binanceClient;
        //private readonly List<Prices> _currentPriceList;

        public Services(Client binanceClient)
        {
            _binanceClient = binanceClient;
            //_currentPriceList = ListPrices();

        }

        //Get account information
        public AccountInfo GetAccountAsync()
        {
            var task = Task.Run(async () => await _binanceClient.GetSignedAsync<dynamic>("v3/account"));
            dynamic result = task.Result;
            if (result == null)
                return null;
            AccountInfo accountInfo = JsonConvert.DeserializeObject<AccountInfo>(result.ToString());
            return accountInfo;
        }
        public double GetBalanceFree(string Asset)
        {
            var accountinfo = GetAccountAsync();
            if(accountinfo==null)
                return 0;
            var assetfree = accountinfo.balances.First(x => x.asset == Asset);
            return assetfree.free;

        }
        public List<Order> GetOpenOrdersAsync()
        {
            List<Order> orders = new List<Order>();
            var task = Task.Run(async () => await _binanceClient.GetSignedAsync<dynamic>("v3/openOrders"));
            dynamic result = task.Result;
            orders = JsonConvert.DeserializeObject<List<Order>>(result.ToString());
            return orders;
        }

        public SystemStatus GetSystemStatus()
        {
            var task = Task.Run(async () => await _binanceClient.GetAsyncSapi<dynamic>("v1/system/status"));
            dynamic result = task.Result;
            var sistemstatus = JsonConvert.DeserializeObject<SystemStatus>(result.ToString());
            return sistemstatus;
        }



        //Test LIMIT order
        public async Task<dynamic> PlaceTestOrderAsync(string symbol, string side, double quantity, double price)
        {

            var result = await _binanceClient.PostSignedAsync<dynamic>("v3/order/test", "symbol=" + symbol + "&" + "side=" + side + "&" + "type=LIMIT" + "&" + "quantity=" + quantity.ToString() + "&" + "price=" + price.ToString() + "&" + "timeInForce=GTC" + "&" + "recvWindow=6000");
            if (result == null)
            {
                throw new NullReferenceException();
            }

            return result;
        }


//        {
//  "symbol": "LTCBTC",
//  "origClientOrderId": "myOrder1",
//  "orderId": 4,
//  "orderListId": -1, //Unless part of an OCO, the value will always be -1.
//  "clientOrderId": "cancelMyOrder1",
//  "price": "2.00000000",
//  "origQty": "1.00000000",
//  "executedQty": "0.00000000",
//  "cummulativeQuoteQty": "0.00000000",
//  "status": "CANCELED",
//  "timeInForce": "GTC",
//  "type": "LIMIT",
//  "side": "BUY"
//}
    //Place a BUY order, defaults to LIMIT if type is not specified
    public async Task<dynamic> PlaceBuyOrderAsync(string symbol, double quantity, double price, string type = "LIMIT")
        {
            var result = await _binanceClient.PostSignedAsync<dynamic>("v3/order", "symbol=" + symbol + "&" + "side=BUY" + "&" + "type=" + type + "&" + "quantity=" + quantity.ToString() + "&" + "price=" + price.ToString() + "&" + "timeInForce=GTC" + "&" + "recvWindow=6000");
            if (result == null)
            {
                throw new NullReferenceException();
            }

            return result;
        }

        public Order CreateBuyOrder(string symbol, string quantity, string price, string type = "LIMIT")
        {
            Order order = null;
            string query = "symbol=" + symbol + "&" + "side=BUY" + "&" + "type=" + type + "&" + "quantity=" + quantity.ToString() + "&" + "price=" + price.ToString() + "&" + "timeInForce=GTC" + "&" + "recvWindow=6000";
            try
            {
                var task = Task.Run(async () => await _binanceClient.PostSignedAsync<dynamic>("v3/order", query));
                dynamic result = task.Result;
                if (result == null)
                    return null;
                order = JsonConvert.DeserializeObject<Order>(result.ToString());
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
            return order;
        }
        public Order CreateMarketBuyOrder(string symbol, double quoteOrderQty, UsersBudget usersbudget, double coinprice)
        {
            Order order = null;
            quoteOrderQty = Convert.ToDouble(quoteOrderQty.ToString("F8"));
            if (usersbudget.IsAlive==0)
            {
                //var prices = GetTickerPrice(symbol);
                var price = coinprice;    //prices.Find(x => x.Symbol == symbol);

                return new Order() { cummulativeQuoteQty = quoteOrderQty, executedQty = quoteOrderQty / price };
            }
            string query = "symbol=" + symbol + "&" + "side=BUY" + "&" + "type=MARKET&" + "quoteOrderQty=" + quoteOrderQty.ToString("F8").Replace(",",".") + "&"  + "recvWindow=6000";
            try
            {
                var task = Task.Run(async () => await _binanceClient.PostSignedAsync<dynamic>("v3/order", query));
                dynamic result = task.Result;
                if (result == null)
                    return null;
                order = JsonConvert.DeserializeObject<Order>(result.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{usersbudget.UserName}-"+ ex.ToString());
                return null;
            }
            return order;
        }
        //Place a SELL order, defaults to LIMIT if type is not specified
        public Order CreateSellOrder(string symbol, string quantity, string price, string type = "LIMIT")
        {
            Order order = null;
            string query = "symbol=" + symbol + "&" + "side=SELL" + "&" + "type=" + type + "&" + "quantity=" + quantity.ToString() + "&" + "price=" + price.ToString() + "&" + "timeInForce=GTC" + "&" + "recvWindow=6000";
            try
            {
                var task =Task.Run(async()=> await _binanceClient.PostSignedAsync<dynamic>("v3/order", query));
                dynamic result = task.Result;
                if (result == null)
                    return null;
                order = JsonConvert.DeserializeObject<Order>(result.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
            return order;
        }
        //Place a SELL order, defaults to LIMIT if type is not specified
        public Order CreateMarketSellOrder(string symbol, double quantity, UsersBudget usersbudget, double fiyati)
        {
            //Debug.WriteLine($"{symbol} satışı. Adet {quantity}");
            Order order = null;
            if (usersbudget.IsAlive==0)
            {
                //var prices = GetTickerPrice(symbol);
                //if(prices==null)
                //{
                //    Thread.Sleep(2000);
                //    prices = GetTickerPrice(symbol);
                //}
                //if (prices == null)
                //    return null;
                var price = fiyati;  //  prices.Find(x => x.Symbol == symbol);

                return new Order() { cummulativeQuoteQty = quantity*price, executedQty = quantity };
            }
            string query = "symbol=" + symbol + "&" + "side=SELL" + "&" + "type=MARKET&" + "quantity=" + quantity.ToString().Replace(",",".") + "&recvWindow=6000";
            try
            {
                var task = Task.Run(async () => await _binanceClient.PostSignedAsync<dynamic>("v3/order", query));
                dynamic result = task.Result;
                if (result == null)
                    return null;
                order = JsonConvert.DeserializeObject<Order>(result.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{usersbudget.UserName}-"+ex.ToString());
                return null;
            }
            return order;
        }
        //Check an order's status
        public Order GetOrderAsync(string symbol, long orderId)
        {
            string querystring = "symbol=" + symbol + "&" + "orderId=" + orderId.ToString();
            var task = Task.Run(async () => await _binanceClient.GetSignedAsync<dynamic>("v3/order",querystring ));
            dynamic result = task.Result;
            Order orderstat = JsonConvert.DeserializeObject<Order>(result.ToString());
            return orderstat;
        }
        
        //Check an order's status
        public List<Trade> GetTradeAsync(string symbol)
        {
            string querystring = "symbol=" + symbol ;
            var task = Task.Run(async () => await _binanceClient.GetSignedAsync<dynamic>("v3/myTrades", querystring));
            dynamic result = task.Result;
            
            var trades = JsonConvert.DeserializeObject<List<Trade>>(result.ToString());
            return trades;
        }

        //Cancel an order
        public bool CancelOrderAsync(string symbol, long orderId)
        {
            var task = Task.Run(async ()=> await _binanceClient.DeleteSignedAsync<dynamic>("v3/order", "symbol=" + symbol + "&" + "orderId=" + orderId.ToString()));
            dynamic result= task.Result;
            //Order orderstat = JsonConvert.DeserializeObject<Order>(result);
            //if (orderstat == null)
            //    return false;
            //else
                return true;
        }

        //return a List of Price information for consumption
        public List<Prices> ListPrices(dynamic response)
        {
            List<Prices> prices = new List<Prices>();
            prices = JsonConvert.DeserializeObject<List<Prices>>(response.ToString());
            return prices;

        }
        //Overload for ease of use
        public List<Prices> ListPrices()
        {
            List<Prices> prices = new List<Prices>();
            var task = Task.Run(async () => await _binanceClient.GetAsync<dynamic>("v1/ticker/allPrices"));
            dynamic result = task.Result;
            prices = JsonConvert.DeserializeObject<List<Prices>>(result.ToString());
            return prices;

        }

        public double GetPriceOfSymbol(string symbol)
        {
            List<Prices> prices = new List<Prices>();
            var task = Task.Run(async () => await _binanceClient.GetAsync<dynamic>("v1/ticker/allPrices"));
            dynamic result = task.Result;

            prices = ListPrices(result);

            double priceOfSymbol = (from p in prices
                                    where p.Symbol == symbol
                                    select p.Price).First();

            return priceOfSymbol;
        }

        public List<Candlestick> GetKlines(string symbol, string interval, int limit, long? startTime=null, long? endTime=null)
        {
            //var result = await _binanceClient.GetAsync<dynamic>("v3/klines", "symbol=" + symbol + "&" + "interval=" + interval + "&limit=" + limit.ToString());
            //if (result == null)
            //{
            //    throw new NullReferenceException();
            //}
            string querystring = "";
            if (startTime == null && endTime == null)
                querystring = "symbol=" + symbol + "&" + "interval=" + interval + "&limit=" + limit.ToString();
            else if(startTime!=null && endTime==null)
                querystring = "symbol=" + symbol + "&" + "interval=" + interval + "&startTime=" + startTime.ToString() + "&limit=" + limit.ToString();
            else if(startTime == null && endTime != null)
                querystring = "symbol=" + symbol + "&" + "interval=" + interval + "&endTime=" + endTime.ToString() + "&limit=" + limit.ToString();
            else
                querystring = "symbol=" + symbol + "&" + "interval=" + interval + "&startTime=" + startTime.ToString() + "&endTime=" + endTime.ToString() + "&limit=" + limit.ToString();
            var task = Task.Run(async () => await _binanceClient.GetAsync<dynamic>("v3/klines", querystring));
            try
            {
                dynamic result = task.Result;
            }
            catch (Exception)
            {

                return null;
            }
            dynamic result3 = task.Result;

            var result2 = new List<Candlestick>();

            foreach (JToken item in ((JArray)result3).ToArray())
            {
                result2.Add(new Candlestick()
                {
                    OpenTime = long.Parse(item[0].ToString()),
                    Open = float.Parse(item[1].ToString().Replace('.', ',')),
                    High = float.Parse(item[2].ToString().Replace('.', ',')),
                    Low = float.Parse(item[3].ToString().Replace('.', ',')),
                    Close = float.Parse(item[4].ToString().Replace('.', ',')),
                    Volume = float.Parse(item[5].ToString().Replace('.', ',')),
                    CloseTime = long.Parse(item[6].ToString()),
                    QuoteAssetVolume = float.Parse(item[7].ToString().Replace('.', ',')),
                    NumberOfTrades = int.Parse(item[8].ToString()),
                    TakerBuyBaseAssetVolume = float.Parse(item[9].ToString().Replace('.', ',')),
                    TakerBuyQuoteAssetVolume = float.Parse(item[10].ToString().Replace('.', ','))
                });
            }

            return result2;
        }

        public List<Prices> GetTickerPrice(string symbol = null)
        {
            List<Prices> prices = new List<Prices>();
            Prices price = null;
            try
            {
                if (symbol == null)
                {
                    var task = Task.Run(async () => await _binanceClient.GetAsync<dynamic>("v3/ticker/price"));
                    dynamic result = task.Result;
                    prices = JsonConvert.DeserializeObject<List<Prices>>(result.ToString());
                    return prices;
                }
                else
                {
                    var task = Task.Run(async () => await _binanceClient.GetAsync<dynamic>("v3/ticker/price", "symbol=" + symbol));
                    dynamic result = task.Result;
                    price = JsonConvert.DeserializeObject<Prices>(result.ToString());
                    prices.Add(price);
                    return prices;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("GetTickerPrice:");
                Debug.WriteLine(ex.ToString());
            }
            return prices;

        }
        public ExchangeInfo exchangeInfo;
        int exchangeinfo_lastread;
        public async Task GetExchangeInfo()
        {
            
            if (exchangeInfo != null && exchangeinfo_lastread==DateTime.Now.Hour)
                return;
            exchangeinfo_lastread = DateTime.Now.Hour;
            exchangeInfo = new ExchangeInfo();
            var task = await _binanceClient.GetAsync<dynamic>("v3/exchangeInfo");
            //dynamic result = task.Result;
            exchangeInfo = JsonConvert.DeserializeObject<ExchangeInfo>(task.ToString()); // result.ToString());
            //var c = exchangeInfo;

        }
        public ExchangeInfo GetExchangeInfoFull()
        {
            ExchangeInfo exchangeInfo = null;

            var task=Task.Run(async()=>await _binanceClient.GetAsync<dynamic>("v3/exchangeInfo"));
            dynamic result = task.Result;
            exchangeInfo = JsonConvert.DeserializeObject<ExchangeInfo>(result.ToString());
            return exchangeInfo;

        }
        public async Task<Symbols> GetSymbolInfo(string symbol)
        {
            await GetExchangeInfo();
            var sonuc = (from p in exchangeInfo.symbols where p.symbol == symbol select p);
            if (sonuc.Count()!=0)
                return sonuc.First();
            return null;
        }
        /// <summary>
        /// Sembol bilgisini döndürür
        /// </summary>
        /// <param name="Symbol">Sembol adı</param>
        /// <returns>Symbols nesnesi</returns>
        public Symbols GetSembolInfo(string Symbol)
        {
            var task = Task.Run(async () => await GetSymbolInfo(Symbol));
            var result = task.Result;
            return result;
        }
        public bool IsTrading(string symbol)
        {
            var task = Task.Run(async () => await GetSymbolInfo(symbol));
            var result = task.Result;
            if (result == null)
                return false;
            if (result.status == "TRADING")
                return true;


            return false;
        }
        ////Get depth of a symbol
        //public async Task<dynamic> GetDepthAsync(string symbol)
        //{
        //    var result = await _binanceClient.GetAsync<dynamic>("v1/depth", "symbol=" + symbol);

        //    if (result == null)
        //    {
        //        throw new NullReferenceException();
        //    }

        //    return result;
        //}

        //Get latest price of all symbols
        //public async Task<dynamic> GetAllPricesAsync()
        //{
        //    // yenisi v3/ticker/price symbol=
        //    var result = await _binanceClient.GetAsync<dynamic>("v1/ticker/allPrices");
        //    if (result == null)
        //    {
        //        throw new NullReferenceException();
        //    }

        //    return result;

        //}

        //Get current positions
        //public async Task<dynamic> GetAccountPositionsAsync()
        //{
        //    var result = await _binanceClient.GetSignedAsync<dynamic>("v3/account");
        //    if (result == null)
        //    {
        //        throw new NullReferenceException();
        //    }

        //    return result;
        //}

        //Get list of open orders
        public List<Order> GetOrdersAsync(string symbol, int limit = 500)
        {
        //WebSocket webSocket=new WebSocket()
            List<Order> orders = new List<Order>();
            var task = Task.Run(async()=> await _binanceClient.GetSignedAsync<dynamic>("v3/allOrders", "symbol=" + symbol + "&" + "limit=" + limit));
            //if (result == null)
            //{
            //    throw new NullReferenceException();
            //}
            //var task = Task.Run(async () => await _binanceClient.GetSignedAsync<dynamic>("v3/openOrders"));
            dynamic result = task.Result;
            orders = JsonConvert.DeserializeObject<List<Order>>(result.ToString());
            return orders;

            //return result;
        }
        
        ////Get list of trades for account
        //public async Task<dynamic> GetTradesAsync(string symbol)
        //{
        //    var result = await _binanceClient.GetSignedAsync<dynamic>("v3/myTrades", "symbol=" + symbol);
        //    if (result == null)
        //    {
        //        throw new NullReferenceException();
        //    }

        //    return result;
        //}



    }
}
