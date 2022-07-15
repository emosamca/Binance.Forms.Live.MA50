using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class fills
    {
        public double price { get; set; }
        public double qty { get; set; }
        public double commission { get; set; }
        public string commissionAsset { get; set; }
    }
}
