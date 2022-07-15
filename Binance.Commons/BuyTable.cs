using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    [Serializable]
    public class BuyTable
    {
        public long Id { get; set; }
        public string CoinName { get; set; }
        public int IkazNo { get; set; }
        public long BuyDate { get; set; }
        public string BuyDatestr { get; set; }
        public double BuyPrice { get; set; }
        public double Price { get; set; }
        public double Profit { get; set; }
        public double MaxProfit { get; set; }
        public double MinProfit { get; set; }
        public string DayProfit { get; set; }
        public long NewBuyLimitDate { get; set; }
        public long SellLimitDate { get; set; }
        public string SellDatestr { get; set; }
        public string Notlar { get; set; }
        public int State { get; set; }
        public double atrsatis { get; set; }
        public double atrstoploss { get; set; }
        public double atrval { get; set; }
        public string atrresult { get; set; }
        public double atrprofit { get; set; }

    }
}
