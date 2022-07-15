using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class BtcStates
    {
        public long id { get; set; }
        public int btcma50up { get; set; }
        public int btcma20up { get; set; }
        public int ethma50up { get; set; }
        public int ethma20up { get; set; }
        public int candlecount { get; set; }
        public int btcma6up { get; set; }
        public double regrnow { get; set; }
        public double redrcheckval { get; set; }
        public double redrcheck { get; set; } // 1 ise 4 saatlik mum sonundadki regrnow redrcheckval olarak set edilir ve burası 0 yapılır.
        public double MA50old { get; set; }
        public double MA6old { get; set; }
        public double MA50 { get; set; }
        public double MA6 { get; set; }
        public bool onlybuystop { get; set; }
        public bool botfullstop { get; set; }


    }
}
