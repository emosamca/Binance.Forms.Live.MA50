using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class Order
    {
        public string symbol { get; set; }
        public long orderId { get; set; }
        public int orderListId { get; set; }
        public string clientOrderId { get; set; }
        public double price { get; set; }
        public double origQty { get; set; }
        public double executedQty { get; set; }
        public double cummulativeQuoteQty { get; set; }
        public string status { get; set; }
        public string timeInForce { get; set; }
        public string type { get; set; }
        public string side { get; set; }
        public double stopPrice { get; set; }
        public double icebergQty { get; set; }
        public long time { get; set; }
        public long updateTime { get; set; }
        public bool isWorking { get; set; }
        public double origQuoteOrderQty { get; set; }
        public double CurrentPrice { get; set; }
        public double TotalPrice { get; set; }
        public double CurrentProfit { get; set; }
        public string UserName { get; set; }
        public string Sure { get; set; }
        public int Queeue { get; set; }
        public long Id { get; set; }
        public int Seviye { get; set; }
        public List<fills> fills { get; set; }

    }
}
