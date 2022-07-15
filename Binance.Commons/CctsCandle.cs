using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class CctsCandle
    {
        public long id { get; set; }
        public double ccts { get; set; }
        public long candleopentime { get; set; }
        public int mincoincount { get; set; }
    }
}
