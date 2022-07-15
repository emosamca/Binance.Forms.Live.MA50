using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class SMASValues : IIndicatorSerie
    {
        public double Last { get; set; }
        public double Last2 { get; set; }
        public double Last4 { get; set; }
        public double Last8 { get; set; }
        public double Last16 { get; set; }
        public double Last32 { get; set; }
        public double Last64 { get; set; }
        public double Last96 { get; set; }
        public double Last2old { get; set; }
        public double Last16old { get; set; }
        public double Last96old { get; set; }

        public double AnalizValue { get; set; }
        public double AnalizValue2 { get; set; }
        public double AnalizValue3 { get; set; }
        public bool AnalizValueBuy { get; set; }
        public double Esik { get; set; }
        public double LMTS { get; set; }
        public bool CloseUygun { get; set; }
        public bool BirOncekiCloseUygun { get; set; }
        public double LastDailyProfit { get; set; }
    }
}
