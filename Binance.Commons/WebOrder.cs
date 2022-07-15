using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class WebOrder
    {
        public long id { get; set; }
        public int emir_kod { get; set; }
        public int state { get; set; }
        public long userid { get; set; }
        public long newbuytableid { get; set; }

        public DateTime order_date { get; set; }
        public double? Parameter { get; set; }
        public string Parameter2 { get; set; }
    }
}
