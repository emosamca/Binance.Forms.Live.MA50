using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class Trade
    {
        //        "symbol": "BNBBTC",
        //"id": 28457,
        //"orderId": 100234,
        //"orderListId": -1, //Unless OCO, the value will always be -1
        //"price": "4.00000100",
        //"qty": "12.00000000",
        //"quoteQty": "48.000012",
        //"commission": "10.10000000",
        //"commissionAsset": "BNB",
        //"time": 1499865549590,
        //"isBuyer": true,
        //"isMaker": false,
        //"isBestMatch": true
        public string symbol { get; set; }
        public long id { get; set; }
        public long orderId { get; set; }
        public long orderListId { get; set; }
        public double price { get; set; }
        public double qty { get; set; }
        public double quoteQty { get; set; }
        public double commision { get; set; }
        public double commisionAsset { get; set; }
        public long time { get; set; }
        public bool isBuyer { get; set; }
        public bool isMaker { get; set; }
        public bool isBestMatch { get; set; }

    }
}
