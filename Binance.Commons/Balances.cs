using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class Balances
    {
        public string asset { get; set; }
        public double free { get; set; }
        public double locked { get; set; }
    }
}
