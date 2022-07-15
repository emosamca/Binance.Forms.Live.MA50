using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class MAcoin
    {
        public int MAtypes { get; set; }
        public bool MA50ustu { get; set; }
        public bool MA100ustu { get; set; }
        public bool MA200ustu { get; set; }
        public string MAorder { get; set; }
        public string Name { get; set; }
        public bool MA50ilk { get; set; }
        public double LastCandleProfit { get; set; }

    }
}
