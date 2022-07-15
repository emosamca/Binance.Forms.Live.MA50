using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Commons;
using MySql.Data;
using MySql.Data.MySqlClient;


namespace Binance.Mysql
{
    public class MysqlClass
    {
        protected MySqlConnection mySqlConnection;
        private object lockobj = new object();
        public MysqlClass()
        {
            mySqlConnection = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'");
            mySqlConnection.Open();
        }

        /// <summary>
        /// Tarama yapmak için kullanılacak coin listesini getirir.
        /// </summary>
        /// <returns>Coin listesi</returns>
        public List<coin> GetCoinList(bool IsActive)
        {
            List<coin> coinlist = new List<coin>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        if(!IsActive)
                            command.CommandText = "SELECT * FROM coinlist;";
                        else
                            command.CommandText = "SELECT * FROM coinlist where active=1;";
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                coin m_coin = new coin();
                                m_coin.id = reader.GetInt64(0);
                                m_coin.name = reader.GetString(1);
                                m_coin.candlestart = reader.GetInt64(2);
                                m_coin.atrval = reader.GetDouble(3);
                                m_coin.active = reader.GetInt32("active") == 1 ? true : false;
                                coinlist.Add(m_coin);
                            }
                            reader.Close();
                        }
                    }
                    myconn.Close();
                }
            }
            return coinlist;
        }
        public double GetRiskFaktorValue()
        {
            double riskfaktorval = 0.80 ;
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM riskfaktor order by id desc limit 1;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                riskfaktorval = reader.GetDouble("riskval");
                            }
                            reader.Close();
                        }
                    }
                    myconn.Close();
                }
            }
            return riskfaktorval;
        }
        public double GetStopLossValue()
        {
            double stoploss = -2.5;
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM riskfaktor order by id desc limit 1;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stoploss = reader.GetDouble("stoploss");
                            }
                            reader.Close();
                        }
                    }
                    myconn.Close();
                }
            }
            return stoploss;
        }
        public double GetBuyPercentValue()
        {
            double buypercent = 0.965;
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM riskfaktor order by id desc limit 1;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                buypercent = reader.GetDouble("buypercent");
                            }
                            reader.Close();
                        }
                    }
                    myconn.Close();
                }
            }
            return buypercent;
        }
        public void UpdateStopLossValue(double stoplossval)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.riskfaktor SET stoploss=@stoploss where id=1;";
                    command.Parameters.AddWithValue("@stoploss", stoplossval);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
        }
        public double GetStopLossCheckValue()
        {
            double stoplosscheck = -0.75;
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM riskfaktor order by id desc limit 1;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stoplosscheck = reader.GetDouble("stoplosscheck");
                            }
                            reader.Close();
                        }
                    }
                    myconn.Close();
                }
            }
            return stoplosscheck;
        }
        public void UpdateStopLossCheckValue(double stoplosscheckval)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.riskfaktor SET stoplosscheck=@stoplosscheck where id=1;";
                    command.Parameters.AddWithValue("@stoploss", stoplosscheckval);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteBannedList()
        {
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM bannedlist where bantime<@bantime;";
                        long bantime = DateHelper.GetCurrentTimeStam() - (60 * 60 * 1000);
                        command.Parameters.AddWithValue("@bantime", bantime);
                        int sayi=command.ExecuteNonQuery();
                        Debug.WriteLine($"{sayi} adet ban temizlendi.");
                    }
                    myconn.Close();
                }
            }

        }
        public List<string> GetBannedList()
        {
            List<string> coinlist = new List<string>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM bannedlist;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var okunan = reader.GetString(1);
                                coinlist.Add(okunan);
                            }
                            reader.Close();
                        }
                    }
                    myconn.Close();
                }
            }
            return coinlist;
        }
        public bool SaveBanlist(string symbol)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = @"INSERT INTO emos.bannedlist (coinname,bantime) VALUES ( @coinname, @bantime);";
                        command.Parameters.AddWithValue("@coinname", symbol);
                        command.Parameters.AddWithValue("@bantime", DateHelper.GetCurrentTimeStam());

                        int rowCount = command.ExecuteNonQuery();
                        if (rowCount > 0)
                            return true;
                    }
                }
            }
            return false;
        }
        public List<WebOrder> GetWebOrders(int state)
        {
            List<WebOrder> webOrders = new List<WebOrder>();
            using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
            {
                myconn.Open();
                using (var command = myconn.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM web_emirler WHERE state={state.ToString().Trim()};";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WebOrder webOrder = new WebOrder();
                            webOrder.id = reader.GetInt64(0);
                            webOrder.emir_kod = reader.GetInt32(1);
                            webOrder.state = reader.GetInt32(2);
                            webOrder.userid = reader.GetInt64(3);
                            webOrder.newbuytableid = reader.GetInt64(4);
                            if (!reader.IsDBNull(6))
                                webOrder.Parameter = reader.GetDouble(6);
                            if (!reader.IsDBNull(7))
                                webOrder.Parameter2 = reader.GetString(7);
                            webOrders.Add(webOrder);
                        }
                        reader.Close();
                    }
                }
                myconn.Close();
            }
            return webOrders;
        }
        public void UpdateWebOrdersState(WebOrder webOrder)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.web_emirler SET state=@state where id=@id;";
                    command.Parameters.AddWithValue("@state", webOrder.state);
                    command.Parameters.AddWithValue("@id", webOrder.id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
        }
        public List<User> GetUsers()
        {
            List<User> users = new List<User>();
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM emos.users;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User();
                            user.Id= reader.GetInt64(0);
                            user.UserName = reader.GetString(1);
                            user.Apikey = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            user.Apisecretkey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            user.Userid = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
                            user.Groupid= reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
                            user.Tradecount = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                            user.Pnlsend = reader.IsDBNull(7) ? false : reader.GetInt32(7) == 1 ? true : false;
                            user.Ordersend = reader.IsDBNull(8) ? false : reader.GetInt32(8) == 1 ? true : false;
                            user.Tradeenable= reader.IsDBNull(9) ? false : reader.GetInt32(9) == 1 ? true : false;
                            user.Htmlheader = reader.IsDBNull(10) ? "" : reader.GetString(10);
                            user.autobot = reader.GetInt32(12);
                            users.Add(user);
                        }
                        reader.Close();
                    }
                }
            }
            return users;
        }
        public bool UpdateUser(User user)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.users SET Tradeenable=@Tradeenable, autobot=@autobot where id=@id;";
                    command.Parameters.AddWithValue("@Tradeenable", user.Tradeenable == false ? 0 : 1);
                    command.Parameters.AddWithValue("@autobot", user.autobot);
                    command.Parameters.AddWithValue("@id", user.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool UpdateCoinlist(coin coininfo)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.coinlist SET name=@name, candlestart=@candlestart, atrval=@atrval, @active=active where id=@id;";
                    command.Parameters.AddWithValue("@name", coininfo.name);
                    command.Parameters.AddWithValue("@candlestart", coininfo.candlestart);
                    command.Parameters.AddWithValue("@atrval", coininfo.atrval);
                    command.Parameters.AddWithValue("@active", coininfo.active);
                    command.Parameters.AddWithValue("@id", coininfo.id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        private void checkDBState()
        {
            if (mySqlConnection.State != System.Data.ConnectionState.Open)
                mySqlConnection.Open();
        }
        public bool SaveCoin(string symbol)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO emos.coinlist (name) VALUES (@name);";
                    command.Parameters.AddWithValue("@name", symbol);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount > 0)
                        return true;
                }
            }
            return false;
        }
        public bool SaveCoins(List<coin> coins)
        {
            checkDBState();
            TruncateCoinList();
            foreach (var item in coins)
            {
                SaveCoin(item.name);
            }
            return true;
        }
        public bool DeleteCoin(string symbol)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM emos.coinlist WHERE (name = @name );";
                    command.Parameters.AddWithValue("@name", symbol);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount > 0)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Gece 03:00 da kaydedilen pnl
        /// </summary>
        /// <param name="isim">PNL si çekilecek kullanıcı</param>
        /// <returns>Son PNL</returns>
        public PNL GetOpenPNL(string isim)
        {
            PNL pnl = null;
            lock (lockobj)
            {
                MySqlConnection m_mySqlConnection = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'");
                m_mySqlConnection.Open();
                using (var command = m_mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM emos.pnl WHERE name='{isim}' ORDER BY tarih DESC LIMIT 1;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pnl = new PNL();
                            pnl.Name = reader.GetString(1);
                            pnl.Tarih = reader.GetInt64(2);
                            pnl.Dolar = reader.GetDouble(4);
                            pnl.Btc = reader.GetDouble(5);
                            pnl.TRY = reader.GetDouble(6);
                        }
                        reader.Close();
                    }
                }
                m_mySqlConnection.Close();
                m_mySqlConnection.Dispose();
            }
            return pnl;
        }
        /// <summary>
        /// PNL yi veritabanına kaydeder
        /// </summary>
        /// <param name="pnl">Kaydedilecek PNL</param>
        /// <returns>Kayıt yaptımı?</returns>
        public bool SavePNL(PNL pnl)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO emos.pnl (name, tarih, tarihstr, dolar, btc, try) VALUES (@name, @tarih,@tarihstr, @dolar, @btc, @try);";
                    command.Parameters.AddWithValue("@name", pnl.Name);
                    command.Parameters.AddWithValue("@tarih", pnl.Tarih);
                    command.Parameters.AddWithValue("@tarihstr", pnl.Tarihstr);
                    command.Parameters.AddWithValue("@dolar", pnl.Dolar);
                    command.Parameters.AddWithValue("@btc", pnl.Btc);
                    command.Parameters.AddWithValue("@try", pnl.TRY);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount > 0)
                        return true;
                }
            }
            return false;
        }

        public bool SaveCCTScandle(CctsCandle cctscandle)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO emos.cctshistory (ccts, candleopentime, mincoincount) VALUES (@ccts, @candleopentime, @mincoincount);";
                    command.Parameters.AddWithValue("@ccts", cctscandle.ccts);
                    command.Parameters.AddWithValue("@candleopentime", cctscandle.candleopentime);
                    command.Parameters.AddWithValue("@mincoincount", cctscandle.mincoincount);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public List<CctsCandle> GetCCTScandle()
        {
            List<CctsCandle> cctsCandles = new List<CctsCandle>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT *  FROM emos.cctshistory;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CctsCandle m_cctcscandle = new CctsCandle();
                                m_cctcscandle.id= reader.GetInt64("id");
                                m_cctcscandle.ccts = reader.GetDouble("ccts");
                                m_cctcscandle.candleopentime = reader.GetInt64("candleopentime");
                                m_cctcscandle.mincoincount = reader.GetInt32("mincoincount");
                                cctsCandles.Add(m_cctcscandle);
                            }
                            reader.Close();
                        }
                    }
                    myconn.Close();
                }
            }
            return cctsCandles;
        }
        public bool DeleteCCTScandle(CctsCandle cctsCandle)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM emos.cctshistory WHERE (id = @id );";
                    command.Parameters.AddWithValue("@id", cctsCandle.id);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount > 0)
                        return true;
                }
            }
            return false;
        }
        public List<Candlestick> GetCoinDurum(string coinname)
        {
            //SELECT name,from_unixtime(opentime/1000) as open_time, open,close,high,low, MTS  FROM emos.coincandle where name='MBOXUSDT' order by opentime ;
            List<Candlestick> candlelist = new List<Candlestick>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT name,from_unixtime(opentime/1000) as open_time, open,close,high,low, MTS  FROM emos.coincandle where name='{coinname}' order by opentime ;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Candlestick m_candle = new Candlestick();
                                m_candle.Name = reader.GetString(0);
                                m_candle.OpenTimeStr = reader.GetString(1);
                                m_candle.Open = (float)reader.GetDouble(2);
                                m_candle.Close = (float)reader.GetDouble(3);
                                m_candle.High = (float)reader.GetDouble(4);
                                m_candle.Low = (float)reader.GetDouble(5);
                                if(!reader.IsDBNull(6))
                                    m_candle.MTS = reader.GetDouble(6);
                                candlelist.Add(m_candle);
                            }
                            reader.Close();
                        }
                    }
                    myconn.Close();
                }
            }
            return candlelist;
        }

        public bool IsSavedCandleStick(string coinname,long opentime)
        {
            checkDBState();
             bool varmi = false;
           lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM emos.coincandle WHERE name='{coinname}' AND opentime={opentime.ToString().Trim()};";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            varmi = true;
                        }
                        reader.Close();
                    }
                }
            }
            return varmi;
        }

        public bool SaveCoinCandle(Candlestick candle)
        {
            checkDBState();
            lock (lockobj)
            {

                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO emos.coincandle (name, opentime, closetime, open, close, high, low) VALUES (@name, @opentime, @closetime, @open, @close, @high, @low);";
                    command.Parameters.AddWithValue("@name", candle.Name);
                    command.Parameters.AddWithValue("@opentime", candle.OpenTime);
                    command.Parameters.AddWithValue("@closetime", candle.CloseTime);
                    command.Parameters.AddWithValue("@open", candle.Open);
                    command.Parameters.AddWithValue("@close", (double)candle.Close);
                    command.Parameters.AddWithValue("@high", candle.High);
                    command.Parameters.AddWithValue("@low", candle.Low);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount > 0)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Verilen isme ait emirleri siler
        /// </summary>
        /// <param name="isim">Emirleri silinecek isim</param>
        /// <returns>Silindi mi?</returns>
        public bool DeleteOrders(string isim)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM emos.orders WHERE (name = @name );";
                    command.Parameters.AddWithValue("@name", isim);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount > 0)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Kullanıcıya ait açık emirler kaydedilir.
        /// </summary>
        /// <param name="name">Kullanıcı adı</param>
        /// <param name="orders">Emir listesi</param>
        /// <returns>Kaydedildi mi?</returns>
        public bool SaveOrders(string name, List<Order> orders)
        {
            checkDBState();
            lock (lockobj)
            {
                foreach (var item in orders)
                {
                    using (var command = mySqlConnection.CreateCommand())
                    {
                        command.CommandText = @"INSERT INTO emos.orders (name, symbol, orderId, side, price) VALUES (@name, @symbol, @orderId, @side, @price);";
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@symbol", item.symbol);
                        command.Parameters.AddWithValue("@orderId", item.orderId);
                        command.Parameters.AddWithValue("@side", item.side);
                        command.Parameters.AddWithValue("@price", item.price);

                        int rowCount = command.ExecuteNonQuery();

                    }
                }
            }
            return true;
        }
        public List<Order> GetOrders(string isim)
        {
            List<Order> orders = new List<Order>();
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM emos.orders WHERE name='{isim}';";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = new Order();
                            order.symbol = reader.GetString(2);
                            order.orderId = reader.GetInt64(3);
                            order.side = reader.GetString(4);
                            order.price = reader.GetDouble(5);
                            orders.Add(order);
                        }
                        reader.Close();
                    }
                }
            }
            return orders;
        }

        public List<AnalizTrack> GetAnalizTrack(string interval, string state, long? startdate=null)
        {
            List<AnalizTrack> analizTracks = new List<AnalizTrack>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        if(startdate==null)
                            command.CommandText = $"SELECT * FROM emos.analiz WHERE sure='{interval}' AND state='{state}';";
                        else
                            command.CommandText = $"SELECT * FROM emos.analiz WHERE state='{state}' AND selldate>={startdate};";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AnalizTrack analizTrack = new AnalizTrack();
                                analizTrack.Id = reader.GetInt64(0);
                                analizTrack.Date = reader.GetInt64(1);
                                analizTrack.Datestr = reader.GetString(2);
                                analizTrack.Sure = reader.GetString(3);
                                analizTrack.Name = reader.GetString(4);
                                analizTrack.Price = reader.GetDouble(5);
                                analizTrack.State = reader.GetString(6);
                                analizTrack.SellPrice = reader.GetDouble(7);
                                analizTrack.Profit = reader.GetDouble(8);
                                analizTrack.MaxPrice = reader.GetDouble(9);
                                analizTrack.MaxProfit = reader.GetDouble(10);
                                analizTrack.CurrentPrice = reader.GetDouble(11);
                                analizTrack.CurrentProfit = reader.GetDouble(12);
                                analizTrack.Volort = reader.GetDouble(13);
                                analizTrack.AlimVolort = reader.GetDouble(14);
                                analizTrack.Changed = false;
                                analizTrack.AlimType = reader.GetInt32(15);
                                analizTrack.Selldate = reader.GetInt64(16);
                                if (reader.IsDBNull(17))
                                    analizTrack.Selldatestr = "";
                                else
                                    analizTrack.Selldatestr = reader.GetString(17);
                                if (reader.IsDBNull(18))
                                    analizTrack.Levels = "";
                                else
                                    analizTrack.Levels = reader.GetString(18);

                                analizTracks.Add(analizTrack);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return analizTracks;
        }

        public bool SaveAnalizTrack(AnalizTrack analiztrack)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO emos.analiz (date, datestr, sure, name, price, state, sellprice,profit,maxprice,maxprofit,currentprice,currentprofit,volort,alimvolort,alimtype,selldate,selldatestr, notlar) VALUES (@date, @datestr, @sure, @name, @price, @state, @sellprice,@profit,@maxprice,@maxprofit,@currentprice,@currentprofit,@volort,@alimvolort,@alimtype,@selldate,@selldatestr, @notlar);";
                    command.Parameters.AddWithValue("@date", analiztrack.Date);
                    command.Parameters.AddWithValue("@datestr", analiztrack.Datestr);
                    command.Parameters.AddWithValue("@sure", analiztrack.Sure);
                    command.Parameters.AddWithValue("@name", analiztrack.Name);
                    command.Parameters.AddWithValue("@price", analiztrack.Price);
                    command.Parameters.AddWithValue("@state", analiztrack.State);
                    command.Parameters.AddWithValue("@sellprice", analiztrack.SellPrice);
                    command.Parameters.AddWithValue("@profit", analiztrack.Profit);
                    command.Parameters.AddWithValue("@maxprice", analiztrack.MaxPrice);
                    command.Parameters.AddWithValue("@maxprofit", analiztrack.MaxProfit);
                    command.Parameters.AddWithValue("@currentprice", analiztrack.CurrentPrice);
                    command.Parameters.AddWithValue("@currentprofit", analiztrack.CurrentProfit);
                    command.Parameters.AddWithValue("@volort", analiztrack.Volort);
                    command.Parameters.AddWithValue("@alimvolort", analiztrack.AlimVolort);
                    command.Parameters.AddWithValue("@alimtype", analiztrack.AlimType);
                    command.Parameters.AddWithValue("@selldate", analiztrack.Selldate);
                    command.Parameters.AddWithValue("@selldatestr", analiztrack.Selldatestr);
                    command.Parameters.AddWithValue("@notlar", analiztrack.Levels);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool UpdateAnalizTrack(AnalizTrack analiztrack)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.analiz SET date=@date, datestr=@datestr, sure=@sure, name=@name, price=@price, state=@state, sellprice=@sellprice,profit=@profit,maxprice=@maxprice,maxprofit= @maxprofit, currentprice=@currentprice, currentprofit=@currentprofit, volort=@volort, alimvolort=@alimvolort, alimtype=@alimtype, selldate=@selldate, selldatestr=@selldatestr, notlar=@notlar where id=@id;";
                    command.Parameters.AddWithValue("@date", analiztrack.Date);
                    command.Parameters.AddWithValue("@datestr", analiztrack.Datestr);
                    command.Parameters.AddWithValue("@sure", analiztrack.Sure);
                    command.Parameters.AddWithValue("@name", analiztrack.Name);
                    command.Parameters.AddWithValue("@price", analiztrack.Price);
                    command.Parameters.AddWithValue("@state", analiztrack.State);
                    command.Parameters.AddWithValue("@sellprice", analiztrack.SellPrice);
                    command.Parameters.AddWithValue("@profit", analiztrack.Profit);
                    command.Parameters.AddWithValue("@maxprice", analiztrack.MaxPrice);
                    command.Parameters.AddWithValue("@maxprofit", analiztrack.MaxProfit);
                    command.Parameters.AddWithValue("@currentprice", analiztrack.CurrentPrice);
                    command.Parameters.AddWithValue("@currentprofit", analiztrack.CurrentProfit);
                    command.Parameters.AddWithValue("@volort", analiztrack.Volort);
                    command.Parameters.AddWithValue("@alimvolort", analiztrack.AlimVolort);
                    command.Parameters.AddWithValue("@alimtype", analiztrack.AlimType);
                    command.Parameters.AddWithValue("@selldate", analiztrack.Selldate);
                    command.Parameters.AddWithValue("@selldatestr", analiztrack.Selldatestr);
                    command.Parameters.AddWithValue("@notlar", analiztrack.Levels);
                    command.Parameters.AddWithValue("@id", analiztrack.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public List<BuyTable> GetBuyTables(int state,string TableName)
        {
            List<BuyTable> buyTables = new List<BuyTable>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        if(state==1)
                            command.CommandText = $"SELECT * FROM emos.{TableName} WHERE state='{state}';";
                        else if(state==0)
                            command.CommandText = $"SELECT * FROM emos.{TableName} WHERE state='{state}' ORDER BY selllimitdate DESC LIMIT 500;";
                        else if (state==3)
                            command.CommandText = $"SELECT * FROM emos.{TableName};";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BuyTable buyTable = new BuyTable();
                                buyTable.Id = reader.GetInt64(0);
                                buyTable.CoinName = reader.GetString(1);
                                buyTable.IkazNo = reader.GetInt32(2);
                                buyTable.BuyDate = reader.GetInt64(3);
                                buyTable.BuyDatestr = reader.GetString(4);
                                buyTable.BuyPrice = reader.GetDouble(5);
                                buyTable.Price = reader.GetDouble(6);
                                buyTable.Profit = reader.GetDouble(7);
                                buyTable.MaxProfit = reader.GetDouble(8);
                                buyTable.MinProfit = reader.GetDouble(9);
                                buyTable.DayProfit = reader.GetString(10);
                                buyTable.NewBuyLimitDate = reader.GetInt64(11);
                                buyTable.SellLimitDate = reader.GetInt64(12);
                                buyTable.SellDatestr = reader.GetString(13);
                                buyTable.Notlar = reader.GetString(14);
                                buyTable.State = reader.GetInt32(15);
                                buyTable.atrsatis = reader.GetDouble(16);
                                buyTable.atrstoploss = reader.GetDouble(17);
                                buyTable.atrval = reader.GetDouble(18);
                                if (!reader.IsDBNull(19))
                                    buyTable.atrresult = reader.GetString(19);
                                buyTable.atrprofit = reader.GetDouble(20);
                                buyTables.Add(buyTable);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return buyTables;
        }
        public bool SaveBuyTable(BuyTable buyTable,string Tablename)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO emos.{Tablename} (coinname, ikazno, buydate, buydatestr, buyprice, price, profit, maxprofit, minprofit, dayprofit, newbuylimitdate, selllimitdate, selldatestr, notlar, state, atrsatis, atrstoploss, atrval, atrresult, atrprofit) VALUES (@coinname, @ikazno, @buydate, @buydatestr, @buyprice, @price, @profit, @maxprofit, @minprofit, @dayprofit, @newbuylimitdate, @selllimitdate, @selldatestr, @notlar, @state,@atrsatis, @atrstoploss, @atrval, @atrresult, @atrprofit);";
                    command.Parameters.AddWithValue("@coinname", buyTable.CoinName);
                    command.Parameters.AddWithValue("@ikazno", buyTable.IkazNo);
                    command.Parameters.AddWithValue("@buydate", buyTable.BuyDate);
                    command.Parameters.AddWithValue("@buydatestr", buyTable.BuyDatestr);
                    command.Parameters.AddWithValue("@buyprice", buyTable.BuyPrice);
                    command.Parameters.AddWithValue("@price", buyTable.Price);
                    command.Parameters.AddWithValue("@profit", buyTable.Profit);
                    command.Parameters.AddWithValue("@maxprofit", buyTable.MaxProfit);
                    command.Parameters.AddWithValue("@minprofit", buyTable.MinProfit);
                    command.Parameters.AddWithValue("@dayprofit", buyTable.DayProfit);
                    command.Parameters.AddWithValue("@newbuylimitdate", buyTable.NewBuyLimitDate);
                    command.Parameters.AddWithValue("@selllimitdate", buyTable.SellLimitDate);
                    command.Parameters.AddWithValue("@selldatestr", buyTable.SellDatestr);
                    command.Parameters.AddWithValue("@notlar", buyTable.Notlar);
                    command.Parameters.AddWithValue("@state", buyTable.State);
                    command.Parameters.AddWithValue("@atrsatis", buyTable.atrsatis);
                    command.Parameters.AddWithValue("@atrstoploss", buyTable.atrstoploss);
                    command.Parameters.AddWithValue("@atrval", buyTable.atrval);
                    command.Parameters.AddWithValue("@atrresult", buyTable.atrresult);
                    command.Parameters.AddWithValue("@atrprofit", buyTable.atrprofit);

                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public bool UpdateBuyTable(BuyTable buyTable,string Tablename)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"UPDATE emos.{Tablename} SET coinname=@coinname, ikazno=@ikazno, buydate=@buydate, buydatestr=@buydatestr, buyprice=@buyprice, price=@price, profit=@profit, maxprofit=@maxprofit, minprofit=@minprofit, dayprofit=@dayprofit, newbuylimitdate=@newbuylimitdate, selllimitdate=@selllimitdate, selldatestr=@selldatestr, notlar=@notlar, state=@state, atrsatis=@atrsatis, atrstoploss=@atrstoploss, atrval=@atrval, atrresult=@atrresult, atrprofit=@atrprofit where id=@id;";
                    command.Parameters.AddWithValue("@coinname", buyTable.CoinName);
                    command.Parameters.AddWithValue("@ikazno", buyTable.IkazNo);
                    command.Parameters.AddWithValue("@buydate", buyTable.BuyDate);
                    command.Parameters.AddWithValue("@buydatestr", buyTable.BuyDatestr);
                    command.Parameters.AddWithValue("@buyprice", buyTable.BuyPrice);
                    command.Parameters.AddWithValue("@price", buyTable.Price);
                    command.Parameters.AddWithValue("@profit", buyTable.Profit);
                    command.Parameters.AddWithValue("@maxprofit", buyTable.MaxProfit);
                    command.Parameters.AddWithValue("@minprofit", buyTable.MinProfit);
                    command.Parameters.AddWithValue("@dayprofit", buyTable.DayProfit);
                    command.Parameters.AddWithValue("@newbuylimitdate", buyTable.NewBuyLimitDate);
                    command.Parameters.AddWithValue("@selllimitdate", buyTable.SellLimitDate);
                    command.Parameters.AddWithValue("@selldatestr", buyTable.SellDatestr);
                    command.Parameters.AddWithValue("@notlar", buyTable.Notlar);
                    command.Parameters.AddWithValue("@state", buyTable.State);
                    command.Parameters.AddWithValue("@id", buyTable.Id);
                    command.Parameters.AddWithValue("@atrsatis", buyTable.atrsatis);
                    command.Parameters.AddWithValue("@atrstoploss", buyTable.atrstoploss);
                    command.Parameters.AddWithValue("@atrval", buyTable.atrval);
                    command.Parameters.AddWithValue("@atrresult", buyTable.atrresult);
                    command.Parameters.AddWithValue("@atrprofit", buyTable.atrprofit);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public bool TruncateIzleme()
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"TRUNCATE TABLE emos.izleme;";
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool TruncateCoinList()
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"TRUNCATE TABLE emos.coinlist;";
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool SaveIzleme(Izleme izleme)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO emos.izleme (symbol, sure, volort, alimort, date, datestr, mesaj) VALUES (@symbol, @sure, @volort, @alimort, @date, @datestr, @mesaj);";
                    command.Parameters.AddWithValue("@symbol", izleme.Symbol);
                    command.Parameters.AddWithValue("@sure", izleme.Interval);
                    command.Parameters.AddWithValue("@volort", izleme.Volort);
                    command.Parameters.AddWithValue("@alimort", izleme.Alimort);
                    command.Parameters.AddWithValue("@date", izleme.Date);
                    command.Parameters.AddWithValue("@datestr", izleme.Datestr);
                    command.Parameters.AddWithValue("@mesaj", izleme.Mesaj);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public List<Izleme> GetIzleme()
        {
            List<Izleme> izlemes = new List<Izleme>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.izleme;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Izleme izleme = new Izleme()
                                {
                                    Id=reader.GetInt64(0),
                                    Symbol = reader.GetString(1),
                                    Interval = reader.GetString(2),
                                    Volort=reader.GetDouble(3),
                                    Alimort=reader.GetDouble(4),
                                    Date=reader.GetInt64(5),
                                    Datestr=reader.GetString(6),
                                    Mesaj=reader.GetString(7)
                                    
                                };
                                izlemes.Add(izleme);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return izlemes;
        }

        public bool DeleteIzleme(Izleme izleme)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM emos.izleme WHERE (id = @id );";
                    command.Parameters.AddWithValue("@id", izleme.Id);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount > 0)
                        return true;
                }
            }
            return false;
        }
        public bool SaveUsersGetInfo(UsersGetInfo usersGetInfo)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO emos.usersgetinfo (username, symbol, sure, date, state, datestr) VALUES (@username, @symbol, @sure, @date, @state, @datestr);";
                    command.Parameters.AddWithValue("@username", usersGetInfo.Username);
                    command.Parameters.AddWithValue("@symbol", usersGetInfo.Symbol);
                    command.Parameters.AddWithValue("@sure", usersGetInfo.Sure);
                    command.Parameters.AddWithValue("@date", usersGetInfo.Date);
                    command.Parameters.AddWithValue("@state", usersGetInfo.State);
                    command.Parameters.AddWithValue("@datestr", usersGetInfo.Datestr);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        /// <summary>
        /// Veritabanında tarama sonucunda alım emri var mı?
        /// </summary>
        /// <param name="isim">Kullanıcı adı</param>
        /// <param name="state">0 alınmamışlar, 1 alınmışlar</param>
        /// <returns>Alınacak coin listesi</returns>
        public List<UsersGetInfo> GetUsersGetInfos(string isim, int state)
        {
            List<UsersGetInfo> usersGetInfos = new List<UsersGetInfo>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.usersgetinfo WHERE username=@username AND state=@state;";
                        command.Parameters.AddWithValue("@username", isim);
                        command.Parameters.AddWithValue("@state", state);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UsersGetInfo usersGetInfo = new UsersGetInfo();
                                usersGetInfo.Id = reader.GetInt64(0);
                                usersGetInfo.Username = reader.GetString(1);
                                usersGetInfo.Symbol = reader.GetString(2);
                                usersGetInfo.Sure = reader.GetString(3);
                                usersGetInfo.Date= reader.GetInt64(4);
                                usersGetInfo.State = reader.GetInt32(5);
                                usersGetInfo.Datestr = reader.GetString(6);
                                usersGetInfos.Add(usersGetInfo);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return usersGetInfos;
        }
        /// <summary>
        /// Kullanıcının alım satım için bütçe bilgileri
        /// </summary>
        /// <param name="isim">Kullanıcı adı</param>
        /// <returns>Bütçe bilgileri</returns>
        public List<UsersBudget> GetUsersBudgets(string isim)
        {
            List < UsersBudget> usersBudgets = new List<UsersBudget>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.usersbudget WHERE username=@username;";
                        command.Parameters.AddWithValue("@username", isim);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var usersBudget = new UsersBudget()
                                {
                                    Id = reader.GetInt64(0),
                                    UserName = reader.GetString(1),
                                    StartBudget = reader.GetDouble(2),
                                    TargetBudget = reader.GetDouble(3),
                                    TradeMax = reader.GetInt32(4),
                                    TradeNow = reader.GetInt32(5),
                                    RemainingMoney = reader.GetDouble(6),
                                    Withdrawmoney = reader.GetDouble(7),
                                    DepositMoney = reader.GetDouble(8),
                                    TradeEnable = reader.GetInt32(9),
                                    TempTradeDisable = reader.GetInt32(10),
                                    IsAlive = reader.GetInt32(11),
                                    HtmlHeader = reader.GetString(12),
                                    Seviyeler = reader.GetString(13),
                                    BudgetName = reader.GetString(14),
                                    TradeTytpe = reader.GetInt32(15),
                                    autobotenable = reader.GetInt32(23) == 1 ? true : false,
                                    bottradeenable = reader.GetInt32(24) == 1 ? true : false,
                                    stoplossenable = reader.GetInt32(25) == 1 ? true : false,
                                    stoplossvalue = reader.GetDouble(26)
                                };
                                usersBudgets.Add(usersBudget);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return usersBudgets;
        }
        public void UpdateUserBadgetAutobot(UsersBudget usersBudget)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersbudget SET autobot=@autobot,tradeenable=@tradeenable, bottradeenable=@bottradeenable where id=@id;";
                    command.Parameters.AddWithValue("@autobot", usersBudget.autobotenable == true ? 1 : 0);
                    command.Parameters.AddWithValue("@tradeenable", usersBudget.TradeEnable);
                    command.Parameters.AddWithValue("@bottradeenable", usersBudget.bottradeenable == true ? 1 : 0);

                    command.Parameters.AddWithValue("@id", usersBudget.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }

        }
        public void UpdateUserBadgetStopLoss(UsersBudget usersBudget)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersbudget SET stoplossenable=@stoplossenable,stoplossvalue=@stoplossvalue where id=@id;";
                    command.Parameters.AddWithValue("@stoplossenable", usersBudget.stoplossenable == true ? 1 : 0);
                    command.Parameters.AddWithValue("@stoplossvalue", usersBudget.stoplossvalue);

                    command.Parameters.AddWithValue("@id", usersBudget.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }

        }
        
        public List<UsersBudget> GetAllUsersBudget()
        {
            List<UsersBudget> usersBudgets =new List<UsersBudget>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.usersbudget;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UsersBudget usersBudget = new UsersBudget()
                                {
                                    Id = reader.GetInt64(0),
                                    UserName = reader.GetString(1),
                                    StartBudget = reader.GetDouble(2),
                                    TargetBudget = reader.GetDouble(3),
                                    TradeMax = reader.GetInt32(4),
                                    TradeNow = reader.GetInt32(5),
                                    RemainingMoney = reader.GetDouble(6),
                                    Withdrawmoney = reader.GetDouble(7),
                                    DepositMoney = reader.GetDouble(8),
                                    TradeEnable = reader.GetInt32(9),
                                    TempTradeDisable = reader.GetInt32(10),
                                    IsAlive = reader.GetInt32(11),
                                    HtmlHeader = reader.GetString(12),
                                    Seviyeler = reader.GetString(13)
                                };
                                usersBudgets.Add(usersBudget);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return usersBudgets;
        }

        public void UpdateUserBadget(UsersBudget usersBudget)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersbudget SET username=@username, startbudget=@startbudget, targetbudget=@targetbudget, trademax=@trademax, tradenow=@tradenow, remainingmoney=@remainingmoney, withdrawmoney=@withdrawmoney, depositmoney=@depositmoney, tradeenable=@tradeenable, temptradedisable=@temptradedisable, isalive=@isalive where id=@id;";
                    command.Parameters.AddWithValue("@username", usersBudget.UserName);
                    command.Parameters.AddWithValue("@startbudget", usersBudget.StartBudget);
                    command.Parameters.AddWithValue("@targetbudget", usersBudget.TargetBudget);
                    command.Parameters.AddWithValue("@trademax", usersBudget.TradeMax);
                    command.Parameters.AddWithValue("@tradenow", usersBudget.TradeNow);
                    command.Parameters.AddWithValue("@remainingmoney", usersBudget.RemainingMoney);
                    command.Parameters.AddWithValue("@withdrawmoney", usersBudget.Withdrawmoney);
                    command.Parameters.AddWithValue("@depositmoney", usersBudget.DepositMoney);
                    command.Parameters.AddWithValue("@tradeenable", usersBudget.TradeEnable);
                    command.Parameters.AddWithValue("@temptradedisable", usersBudget.TempTradeDisable);
                    command.Parameters.AddWithValue("@isalive", usersBudget.IsAlive);
                    
                    command.Parameters.AddWithValue("@id", usersBudget.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateUserBadgetTradeAndMoney(UsersBudget usersBudget)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersbudget SET targetbudget=@targetbudget, tradenow=@tradenow, remainingmoney=@remainingmoney where id=@id;";
                    command.Parameters.AddWithValue("@targetbudget", usersBudget.TargetBudget);
                    command.Parameters.AddWithValue("@tradenow", usersBudget.TradeNow);
                    command.Parameters.AddWithValue("@remainingmoney", usersBudget.RemainingMoney);

                    command.Parameters.AddWithValue("@id", usersBudget.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }

            //RemainingMoney += cummulativeQuoteQty;
            //TradeNow--;
            //TargetBudget
        }
        public void UpdateUserBadgetTrade(UsersBudget usersBudget)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersbudget SET trademax=@trademax, budgettype=@budgettype,remainingmoney=@remainingmoney where id=@id;";
                    command.Parameters.AddWithValue("@trademax", usersBudget.TradeMax);
                    command.Parameters.AddWithValue("@budgettype", usersBudget.TradeTytpe);
                    command.Parameters.AddWithValue("@remainingmoney", usersBudget.RemainingMoney);

                    command.Parameters.AddWithValue("@id", usersBudget.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }

        }
        public void UpdateUserBadgetBalance(UsersBudget usersBudget)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersbudget SET bnbbalance=@bnbbalance, bnbbalanceusdt=@bnbbalanceusdt where id=@id;";
                    command.Parameters.AddWithValue("@bnbbalance", usersBudget.bnbbalance);
                    command.Parameters.AddWithValue("@bnbbalanceusdt", usersBudget.bnbbalanceusdt);
                    command.Parameters.AddWithValue("@id", usersBudget.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }


        }
        public void UpdateUserBadgetWebView(UsersBudget usersBudget)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersbudget SET realstartbudget=@realstartbudget, totalmoney=@totalmoney,realtotalmoney=@realtotalmoney,profit=@profit,realprofit=@realprofit  where id=@id;";
                    command.Parameters.AddWithValue("@realstartbudget", usersBudget.realstartbudget);
                    command.Parameters.AddWithValue("@totalmoney", usersBudget.totalmoney);
                    command.Parameters.AddWithValue("@realtotalmoney", usersBudget.realtotalmoney);
                    command.Parameters.AddWithValue("@profit", usersBudget.profit);
                    command.Parameters.AddWithValue("@realprofit", usersBudget.realprofit);

                    command.Parameters.AddWithValue("@id", usersBudget.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }

        }
        public void UpdateUserBadgetDepositAndWithdraw(UsersBudget usersBudget)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersbudget SET withdrawmoney=@withdrawmoney, depositmoney=@depositmoney, tradeenable=@tradeenable, remainingmoney=@remainingmoney where id=@id;";
                    command.Parameters.AddWithValue("@withdrawmoney", usersBudget.Withdrawmoney);
                    command.Parameters.AddWithValue("@depositmoney", usersBudget.DepositMoney);
                    command.Parameters.AddWithValue("@tradeenable", usersBudget.TradeEnable);
                    command.Parameters.AddWithValue("@remainingmoney", usersBudget.RemainingMoney);

                    command.Parameters.AddWithValue("@id", usersBudget.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }

        }
        public void UpdateUserBadgetButce(UsersBudget usersBudget)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersbudget SET startbudget=@startbudget, remainingmoney=@remainingmoney where id=@id;";
                    command.Parameters.AddWithValue("@startbudget", usersBudget.StartBudget);
                    command.Parameters.AddWithValue("@remainingmoney", usersBudget.RemainingMoney);

                    command.Parameters.AddWithValue("@id", usersBudget.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }

        }
        public int GetCandleCount()
        {
            int mumsay = 0;
            checkDBState();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.states;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mumsay = reader.GetInt32(5);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return mumsay;
        }
        public BtcStates GetBTCma50states()
        {
            BtcStates btcstates = null;
            checkDBState();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.states;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                btcstates = new BtcStates();
                                btcstates.id = reader.GetInt64(0);
                                btcstates.btcma50up = reader.GetInt32(1);
                                btcstates.btcma20up = reader.GetInt32(2);
                                btcstates.ethma50up = reader.GetInt32(3);
                                btcstates.ethma20up = reader.GetInt32(4);
                                btcstates.candlecount = reader.GetInt32(5);
                                btcstates.btcma6up = reader.GetInt32(6);
                                btcstates.regrnow = reader.GetDouble(7);
                                btcstates.redrcheckval = reader.GetDouble(8);
                                btcstates.redrcheck = reader.GetDouble(9);
                                btcstates.MA50old = reader.GetDouble(10);
                                btcstates.MA6old = reader.GetDouble(11);
                                btcstates.MA50 = reader.GetDouble(12);
                                btcstates.MA6 = reader.GetDouble(13);
                                btcstates.onlybuystop = reader.GetInt32(14) == 0 ? false : true;
                                btcstates.botfullstop = reader.GetInt32(15) == 0 ? false : true;
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return btcstates;
        }
        public void UpdateBTCandETHstates(BtcStates btcstates)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.states SET btcma50up=@btcma50up, btcma20up=@btcma20up, ethma50up=@ethma50up, ethma20up=@ethma20up, candlecount=@candlecount, btcma6up=@btcma6up, regrnow=@regrnow, redrcheckval=@redrcheckval, redrcheck=@redrcheck, MA50old=@MA50old, MA6old=@MA6old, MA50=@MA50, MA6=@MA6, onlybuystop=@onlybuystop, botfullstop=@botfullstop where id=@id;";
                    command.Parameters.AddWithValue("@btcma50up", btcstates.btcma50up);
                    command.Parameters.AddWithValue("@btcma20up", btcstates.btcma20up);
                    command.Parameters.AddWithValue("@ethma50up", btcstates.ethma50up);
                    command.Parameters.AddWithValue("@ethma20up", btcstates.ethma20up);
                    command.Parameters.AddWithValue("@candlecount", GlobalVars.CandleCount);
                    command.Parameters.AddWithValue("@btcma6up", btcstates.btcma6up);
                    command.Parameters.AddWithValue("@regrnow", btcstates.regrnow);
                    command.Parameters.AddWithValue("@redrcheckval", btcstates.redrcheckval);
                    command.Parameters.AddWithValue("@redrcheck", btcstates.redrcheck);
                    command.Parameters.AddWithValue("@MA50old", btcstates.MA50old);
                    command.Parameters.AddWithValue("@MA6old", btcstates.MA6old);
                    command.Parameters.AddWithValue("@MA50", btcstates.MA50);
                    command.Parameters.AddWithValue("@MA6", btcstates.MA6);
                    command.Parameters.AddWithValue("@onlybuystop", btcstates.onlybuystop ? 1 : 0);
                    command.Parameters.AddWithValue("@botfullstop", btcstates.botfullstop ? 1 : 0);
                    command.Parameters.AddWithValue("@id", btcstates.id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }

        }
        public void UpdateBTCandETHstatesCandleCount(BtcStates btcstates)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.states SET candlecount=@candlecount where id=@id;";
                    command.Parameters.AddWithValue("@candlecount", GlobalVars.CandleCount);
                    command.Parameters.AddWithValue("@id", btcstates.id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }

        }
        public void UpdateUsersGetInfo(UsersGetInfo usersGetInfo)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.usersgetinfo SET username=@username, symbol=@symbol, sure=@sure, date=@date, state=@state, datestr=@datestr where id=@id;";
                    command.Parameters.AddWithValue("@username", usersGetInfo.Username);
                    command.Parameters.AddWithValue("@symbol", usersGetInfo.Symbol);
                    command.Parameters.AddWithValue("@sure", usersGetInfo.Sure);
                    command.Parameters.AddWithValue("@date", usersGetInfo.Date);
                    command.Parameters.AddWithValue("@state", usersGetInfo.State);
                    command.Parameters.AddWithValue("@datestr", usersGetInfo.Datestr);
                    command.Parameters.AddWithValue("@id", usersGetInfo.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
        }

        public void SaveUserOrders(Order userOrder)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO emos.userorders (username, symbol, orderId, price, origQty, executedQty, status, ordertype, orderside, time, currentprice, totalprice, currentprofit, sure, queeue, seviye) VALUES (@username, @symbol, @orderId, @price, @origQty, @executedQty, @status, @ordertype, @orderside, @time, @currentprice, @totalprice, @currentprofit, @sure, @queeue, @seviye);";
                    command.Parameters.AddWithValue("@username", userOrder.UserName);
                    command.Parameters.AddWithValue("@symbol", userOrder.symbol);
                    command.Parameters.AddWithValue("@orderId", userOrder.orderId);
                    command.Parameters.AddWithValue("@price", userOrder.price);
                    command.Parameters.AddWithValue("@origQty", userOrder.origQty);
                    command.Parameters.AddWithValue("@executedQty", userOrder.executedQty);
                    command.Parameters.AddWithValue("@status", userOrder.status);
                    command.Parameters.AddWithValue("@ordertype", userOrder.type);
                    command.Parameters.AddWithValue("@orderside", userOrder.side);
                    command.Parameters.AddWithValue("@time", userOrder.time);
                    command.Parameters.AddWithValue("@currentprice", userOrder.CurrentPrice);
                    command.Parameters.AddWithValue("@totalprice", userOrder.TotalPrice);
                    command.Parameters.AddWithValue("@currentprofit", userOrder.CurrentProfit);
                    command.Parameters.AddWithValue("@sure", userOrder.Sure);
                    command.Parameters.AddWithValue("@queeue", userOrder.Queeue);
                    command.Parameters.AddWithValue("@seviye", userOrder.Seviye);

                    int rowCount = command.ExecuteNonQuery();
                }
            }
        }

        public List<Order> GetUserOrders(string username, string side, string status)
        {
            List<Order> orders = new List<Order>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.userorders WHERE username=@username AND orderside=@orderside AND status=@status;";
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@orderside", side);
                        command.Parameters.AddWithValue("@status", status);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var order = new Order()
                                {
                                    Id = reader.GetInt64(0),
                                    UserName = reader.GetString(1),
                                    symbol= reader.GetString(2),
                                    orderId = reader.GetInt64(3),
                                    price = reader.GetDouble(4),
                                    origQty = reader.GetDouble(5),
                                    executedQty = reader.GetDouble(6),
                                    status = reader.GetString(7),
                                    type = reader.GetString(8),
                                    side = reader.GetString(9),
                                    time = reader.GetInt64(10),
                                    CurrentPrice = reader.GetDouble(11),
                                    TotalPrice = reader.GetDouble(12),
                                    CurrentProfit = reader.GetDouble(13),
                                    Sure=reader.GetString(14),
                                    Queeue=reader.GetInt32(15),
                                    Seviye=reader.GetInt32(16)
                                };
                                orders.Add(order);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return orders;
        }

        public void UpdateUserOrders(Order order)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = @"UPDATE emos.userorders SET username=@username, symbol=@symbol, orderId=@orderId, price=@price, origQty=@origQty, executedQty=@executedQty, status=@status, ordertype=@ordertype, orderside=@orderside, time=@time, currentprice=@currentprice, totalprice=@totalprice, currentprofit=@currentprofit, sure=@sure, queeue=@queeue, seviye=@seviye  where id=@id;";
                    command.Parameters.AddWithValue("@username", order.UserName);
                    command.Parameters.AddWithValue("@symbol", order.symbol);
                    command.Parameters.AddWithValue("@orderId", order.orderId);
                    command.Parameters.AddWithValue("@price", order.price);
                    command.Parameters.AddWithValue("@origQty", order.origQty);
                    command.Parameters.AddWithValue("@executedQty", order.executedQty);
                    command.Parameters.AddWithValue("@status", order.status);
                    command.Parameters.AddWithValue("@ordertype", order.type);
                    command.Parameters.AddWithValue("@orderside", order.side);
                    command.Parameters.AddWithValue("@time", order.time);
                    command.Parameters.AddWithValue("@currentprice", order.CurrentPrice);
                    command.Parameters.AddWithValue("@totalprice", order.TotalPrice);
                    command.Parameters.AddWithValue("@currentprofit", order.CurrentProfit);
                    command.Parameters.AddWithValue("@sure", order.Sure);
                    command.Parameters.AddWithValue("@queeue", order.Queeue);
                    command.Parameters.AddWithValue("@seviye", order.Seviye);

                    command.Parameters.AddWithValue("@id", order.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
        }
        public List<NewBuyTable> GetNewBuyTables(long id)
        {
            List<NewBuyTable> buyTables = new List<NewBuyTable>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                            command.CommandText = $"SELECT * FROM emos.newbuytable WHERE id='{id}';";
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                NewBuyTable buyTable = new NewBuyTable();
                                int x = 0;
                                buyTable.Id = reader.GetInt64(x++);
                                buyTable.UserName = reader.GetString(x++);
                                buyTable.CoinName = reader.GetString(x++);
                                buyTable.BuyDate = reader.GetInt64(x++);
                                buyTable.BuyDatestr = reader.GetString(x++);
                                buyTable.BuyPrice = reader.GetDouble(x++);
                                buyTable.AlisMTS = reader.GetDouble(x++);
                                buyTable.PriceNow = reader.GetDouble(x++);
                                buyTable.ProfitNow = reader.GetDouble(x++);
                                buyTable.SellPrice = reader.GetDouble(x++);
                                buyTable.SellProfit = reader.GetDouble(x++);
                                buyTable.SellDate = reader.GetInt64(x++);
                                buyTable.SellDatestr = reader.GetString(x++);
                                buyTable.SellCandleMTS = reader.GetDouble(x++);
                                buyTable.SellReason = reader.GetString(x++);
                                buyTable.MaxProfit = reader.GetDouble(x++);
                                buyTable.MinProfit = reader.GetDouble(x++);
                                buyTable.State = reader.GetInt32(x++);
                                buyTable.TotalMoney = reader.GetDouble(x++);
                                buyTable.TotalAdet = reader.GetDouble(x++);
                                if (reader.IsDBNull(x))
                                    buyTable.StopPercent = 0;
                                else
                                {
                                    buyTable.StopPercent = reader.GetDouble(x++);
                                }
                                if (reader.IsDBNull(x))
                                    buyTable.IsStop = false;
                                else
                                {
                                    buyTable.IsStop = reader.GetInt32(x++) == 0 ? false : true;
                                }
                                if (reader.IsDBNull(x))
                                    buyTable.IsLocked= false;
                                else
                                {
                                    buyTable.IsLocked = reader.GetInt32(x++) == 0 ? false : true;
                                }


                                buyTables.Add(buyTable);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return buyTables;
        }
        public List<NewBuyTable> GetNewBuyTables(int state, string username)
        {
            List<NewBuyTable> buyTables = new List<NewBuyTable>();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        if (state == States.Open || state == States.Waiting)
                            command.CommandText = $"SELECT * FROM emos.newbuytable WHERE state='{state}' and username='{username}';";
                        else if (state == States.Sold)
                            command.CommandText = $"SELECT * FROM emos.newbuytable WHERE state='{state}' and username='{username}' ORDER BY selldate DESC LIMIT 500;";
                        else if (state == 3)
                            command.CommandText = $"SELECT * FROM emos.newbuytable  WHERE username='{username}';";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                NewBuyTable buyTable = new NewBuyTable();
                                int x = 0;
                                buyTable.Id = reader.GetInt64(x++);
                                buyTable.UserName = reader.GetString(x++);
                                buyTable.CoinName = reader.GetString(x++);
                                buyTable.BuyDate = reader.GetInt64(x++);
                                buyTable.BuyDatestr = reader.GetString(x++);
                                buyTable.BuyPrice = reader.GetDouble(x++);
                                buyTable.AlisMTS = reader.GetDouble(x++);
                                buyTable.PriceNow = reader.GetDouble(x++);
                                buyTable.ProfitNow = reader.GetDouble(x++);
                                buyTable.SellPrice = reader.GetDouble(x++);
                                buyTable.SellProfit = reader.GetDouble(x++);
                                buyTable.SellDate = reader.GetInt64(x++);
                                buyTable.SellDatestr = reader.GetString(x++);
                                buyTable.SellCandleMTS = reader.GetDouble(x++);
                                buyTable.SellReason = reader.GetString(x++);
                                buyTable.MaxProfit = reader.GetDouble(x++);
                                buyTable.MinProfit = reader.GetDouble(x++);
                                buyTable.State = reader.GetInt32(x++);
                                buyTable.TotalMoney = reader.GetDouble(x++);
                                buyTable.TotalAdet = reader.GetDouble(x++);
                                if (reader.IsDBNull(x))
                                    buyTable.StopPercent = 0;
                                else
                                {
                                    buyTable.StopPercent = reader.GetDouble(x++);
                                }
                                if (reader.IsDBNull(x))
                                    buyTable.IsStop = false;
                                else
                                {
                                    buyTable.IsStop = reader.GetInt32(x++) == 0 ? false : true;
                                }
                                if (reader.IsDBNull(x))
                                    buyTable.IsLocked = false;
                                else
                                {
                                    buyTable.IsLocked = reader.GetInt32(x++) == 0 ? false : true;
                                }
                                x++;
                                buyTable.trackstop = reader.GetDouble(x);
                                buyTables.Add(buyTable);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return buyTables;
        }
        public bool IsThereAnyOpenOrder(string coinname)
        {
            bool isthereorder = false;
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.newbuytable WHERE state=1 and coinname='{coinname}' LIMIT 1;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                isthereorder = true;
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return isthereorder;
        }
        public bool SaveNewBuyTable(NewBuyTable buyTable,string username)
        {
            Debug.WriteLine($"Gelen isim: {username}");
            Debug.WriteLine(buyTable.ToString());
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO emos.newbuytable (username, coinname, buydate, buydatestr, buyprice, alisMTS, pricenow, profitnow, state, totalmoney, totaladet,stop, stop_active, sellreason, sellmtscalc,trackstop ) VALUES (@username, @coinname, @buydate, @buydatestr, @buyprice, @alisMTS, @pricenow, @profitnow, @state, @totalmoney, @totaladet,@stop,@stop_active, @sellreason, @sellmtscalc, @trackstop);";

                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@coinname", buyTable.CoinName);
                    command.Parameters.AddWithValue("@buydate", buyTable.BuyDate);
                    command.Parameters.AddWithValue("@buydatestr", buyTable.BuyDatestr);
                    command.Parameters.AddWithValue("@buyprice", buyTable.BuyPrice);
                    command.Parameters.AddWithValue("@alisMTS", buyTable.AlisMTS);
                    command.Parameters.AddWithValue("@pricenow", buyTable.PriceNow);
                    command.Parameters.AddWithValue("@profitnow", buyTable.ProfitNow);
                    command.Parameters.AddWithValue("@totalmoney", buyTable.TotalMoney);
                    command.Parameters.AddWithValue("@totaladet", buyTable.TotalAdet);
                    command.Parameters.AddWithValue("@state", buyTable.State);
                    command.Parameters.AddWithValue("@sellreason", buyTable.SellReason);
                    command.Parameters.AddWithValue("@sellmtscalc", buyTable.SellLMTS()+buyTable.AlisMTS);
                    command.Parameters.AddWithValue("@stop", buyTable.StopPercent);
                    command.Parameters.AddWithValue("@stop_active", buyTable.IsStop == true ? 1 : 0);
                    command.Parameters.AddWithValue("@trackstop", buyTable.trackstop);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public bool UpdateNewBuyTable(NewBuyTable buyTable)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"UPDATE emos.newbuytable SET coinname=@coinname, buydate=@buydate, buydatestr=@buydatestr, buyprice=@buyprice, alisMTS=@alisMTS, pricenow=@pricenow, profitnow=@profitnow, sellprice=@sellprice, sellprofit=@sellprofit, selldate=@selldate, selldatestr=@selldatestr, sellcandleMTS=@sellcandleMTS, sellreason=@sellreason, maxprofit=@maxprofit, minprofit=@minprofit, state=@state, totalmoney=@totalmoney, totaladet=@totaladet,stop=@stop, stop_active=@stop_active,IsLocked=@IsLocked   where id=@id;";
                    command.Parameters.AddWithValue("@coinname", buyTable.CoinName);
                    command.Parameters.AddWithValue("@buydate", buyTable.BuyDate);
                    command.Parameters.AddWithValue("@buydatestr", buyTable.BuyDatestr);
                    command.Parameters.AddWithValue("@buyprice", buyTable.BuyPrice);
                    command.Parameters.AddWithValue("@alisMTS", buyTable.AlisMTS);
                    command.Parameters.AddWithValue("@pricenow", buyTable.PriceNow);
                    command.Parameters.AddWithValue("@profitnow", buyTable.ProfitNow);

                    command.Parameters.AddWithValue("@sellprice", buyTable.SellPrice);
                    command.Parameters.AddWithValue("@sellprofit", buyTable.SellProfit);
                    command.Parameters.AddWithValue("@selldate", buyTable.SellDate);
                    command.Parameters.AddWithValue("@selldatestr", buyTable.SellDatestr);
                    command.Parameters.AddWithValue("@sellcandleMTS", buyTable.SellCandleMTS);
                    command.Parameters.AddWithValue("@sellreason", buyTable.SellReason);
                    command.Parameters.AddWithValue("@maxprofit", buyTable.MaxProfit);
                    command.Parameters.AddWithValue("@minprofit", buyTable.MinProfit);
                    command.Parameters.AddWithValue("@totalmoney", buyTable.TotalMoney);
                    command.Parameters.AddWithValue("@totaladet", buyTable.TotalAdet);
                    command.Parameters.AddWithValue("@stop", buyTable.StopPercent);
                    command.Parameters.AddWithValue("@stop_active", buyTable.IsStop == true ? 1 : 0);
                    command.Parameters.AddWithValue("@IsLocked", buyTable.IsLocked == true ? 1 : 0);

                    command.Parameters.AddWithValue("@state", buyTable.State);
                    command.Parameters.AddWithValue("@id", buyTable.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool UpdateSellPrice(NewBuyTable buyTable)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"UPDATE emos.newbuytable SET sellprice=@sellprice  where id=@id;";
                    command.Parameters.AddWithValue("@sellprice", buyTable.SellPrice);
                    command.Parameters.AddWithValue("@id", buyTable.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool UpdateOnlyState(NewBuyTable buyTable)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"UPDATE emos.newbuytable SET state=@state,pricenow=@pricenow, profitnow=@profitnow  where id=@id;";
                    command.Parameters.AddWithValue("@state", buyTable.State);
                    command.Parameters.AddWithValue("@pricenow", buyTable.PriceNow);
                    command.Parameters.AddWithValue("@profitnow", buyTable.ProfitNow);
                    command.Parameters.AddWithValue("@id", buyTable.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool UpdateNewBuyTableTrackStop(NewBuyTable buyTable)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"UPDATE emos.newbuytable SET trackstop=@trackstop  where id=@id;";
                    command.Parameters.AddWithValue("@trackstop", buyTable.trackstop);
                    command.Parameters.AddWithValue("@id", buyTable.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool UpdateNewBuyTableReasonAlisMTSExceptState(NewBuyTable buyTable)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"UPDATE emos.newbuytable SET alisMTS=@alisMTS, sellreason=@sellreason where id=@id and state=1;";
                    command.Parameters.AddWithValue("@alisMTS", buyTable.AlisMTS);
                    command.Parameters.AddWithValue("@sellreason", buyTable.SellReason);

                    command.Parameters.AddWithValue("@id", buyTable.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public bool UpdateNewBuyTableExceptState(NewBuyTable buyTable)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"UPDATE emos.newbuytable SET coinname=@coinname, buydate=@buydate, buydatestr=@buydatestr, buyprice=@buyprice, alisMTS=@alisMTS, pricenow=@pricenow, profitnow=@profitnow, sellprice=@sellprice, sellprofit=@sellprofit, selldate=@selldate, selldatestr=@selldatestr, sellcandleMTS=@sellcandleMTS, sellreason=@sellreason, maxprofit=@maxprofit, minprofit=@minprofit, totalmoney=@totalmoney, totaladet=@totaladet  where id=@id and state=1;";
                    command.Parameters.AddWithValue("@coinname", buyTable.CoinName);
                    command.Parameters.AddWithValue("@buydate", buyTable.BuyDate);
                    command.Parameters.AddWithValue("@buydatestr", buyTable.BuyDatestr);
                    command.Parameters.AddWithValue("@buyprice", buyTable.BuyPrice);
                    command.Parameters.AddWithValue("@alisMTS", buyTable.AlisMTS);
                    command.Parameters.AddWithValue("@pricenow", buyTable.PriceNow);
                    command.Parameters.AddWithValue("@profitnow", buyTable.ProfitNow);

                    command.Parameters.AddWithValue("@sellprice", buyTable.SellPrice);
                    command.Parameters.AddWithValue("@sellprofit", buyTable.SellProfit);
                    command.Parameters.AddWithValue("@selldate", buyTable.SellDate);
                    command.Parameters.AddWithValue("@selldatestr", buyTable.SellDatestr);
                    command.Parameters.AddWithValue("@sellcandleMTS", buyTable.SellCandleMTS);
                    command.Parameters.AddWithValue("@sellreason", buyTable.SellReason);
                    command.Parameters.AddWithValue("@maxprofit", buyTable.MaxProfit);
                    command.Parameters.AddWithValue("@minprofit", buyTable.MinProfit);
                    command.Parameters.AddWithValue("@totalmoney", buyTable.TotalMoney);
                    command.Parameters.AddWithValue("@totaladet", buyTable.TotalAdet);

                    command.Parameters.AddWithValue("@id", buyTable.Id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public MTS GetMTS()
        {
            MTS mts = new MTS();
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.mts;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mts.id = reader.GetInt64(0);
                                mts.MTSAl = reader.GetDouble(1);
                                mts.MTSSat= reader.GetDouble(2);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return mts;
        }
        public bool IsTest()
        {
            bool istest = true;
            lock (lockobj)
            {
                using (var myconn = new MySqlConnection("Server=localhost;Database=emos;Uid=emo;Pwd='emoAsdfghjkl73'"))
                {
                    myconn.Open();
                    using (var command = myconn.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM emos.test;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                 int deger= reader.GetInt32(1);
                                if (deger == 1)
                                    istest = true;
                                else
                                    istest = false;
                            }
                            reader.Close();
                        }
                    }
                }
            }
            return istest;
        }
        public bool UpdateMTS(MTS mts)
        {
            checkDBState();
            lock (lockobj)
            {
                using (var command = mySqlConnection.CreateCommand())
                {
                    command.CommandText = $"UPDATE emos.mts SET MTSal=@MTSal, MTSSat=@MTSSat where id=@id;";
                    command.Parameters.AddWithValue("@MTSal", mts.MTSAl);
                    command.Parameters.AddWithValue("@MTSSat", mts.MTSSat);
                    command.Parameters.AddWithValue("@id", mts.id);
                    int rowCount = command.ExecuteNonQuery();
                }
            }
            return true;
        }

    }
}
