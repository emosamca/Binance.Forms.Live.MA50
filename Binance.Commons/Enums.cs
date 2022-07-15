using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    
    public static class Intervals
    {
        public static string OneHour = "1h";
        public static string FourHour = "4h";
        public static string OneDay = "1d";
        public static string FifteenMin = "15m";
        public static string All = "ALL";
    }

    public static class Sides
    {
        public static string Buy = "BUY";
        public static string Sold = "SOLD";
    }

    public static class AlimType
    {
        public static int Normal = 0;
        public static int BesDk = 1;
        public static int Test = 2;
        public static int OkanTest = 3;
    }
    public static class MAType
    {
        public static int MA50ustu = 0;
        public static int MA50alt = 1;
        public static int MA50yeniust = 2;
        public static int MA50yenialt = 3;
    }
    public static class TradeType
    {
        /// <summary>
        /// Eşit
        /// </summary>
        public static int EsitButce = 1;
        /// <summary>
        /// Yarısı
        /// </summary>
        public static int ParaninYarisi = 2;
    }
    public static class WebOrderState
    {
        public static int Open = 1;
        public static int Close = 0;
        public static int Error = 3;
    }
    public static class States
    {
        public static int Sold = 0;
        public static int Open = 1;
        public static int Waiting = 2;
        public static int All = 3;
        public static int Canceled = 4;
        public static int WaitingBuyed = 5;
    }
    public static class WebOrderCode
    {
        public static int Sell = 100;
        public static int SellCoinAll = 101;
        public static int LockCoin = 102;
        public static int UnLockCoin = 103;
        public static int SetStopValue = 104;
        public static int StopCancel = 105;

        public static int SellBudget = 200;
        public static int SellAll = 201;

        public static int TradeSet = 301;
        public static int TradeType = 302;
        public static int PauseTrade = 305;
        public static int ContinueTrade = 306;
        public static int AutobotEnable = 307;
        public static int AutobotDisable = 308;
        public static int PauseAllTrade = 309;
        public static int ContinueAllTrade = 310;

        public static int DepositMoney = 401;
        public static int WithdrawMoney = 402;

        public static int SetStopLossDefault = 501; // tüm kullanıcılar için
        public static int AddCoin = 502;
        public static int DeleteCoin = 503;
        public static int SetStopLossCheckDefault = 504; //-0.75 değeri
        
        public static int SetBudgetStopLossEnable = 505; // 1 = enable, 0 disable
        public static int SetBudgetStopLossValue = 506; // 0 system, !=0 değer
        
    }
}
