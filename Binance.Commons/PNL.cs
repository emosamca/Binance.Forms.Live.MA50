using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class PNL
    {
        public string Name { get; set; }
        public long Tarih { get; set; }
        public double Dolar { get; set; }
        public double Btc { get; set; }
        public double TRY { get; set; }
        public string Tarihstr { get; set; }

        public Percents CalculatePercent(PNL openPNL)
        {
            Percents percents = new Percents();
            percents.DolarPercent = ((Dolar / openPNL.Dolar) - 1) * 100;
            percents.BtcPercent = ((Btc / openPNL.Btc) - 1) * 100;
            percents.TRYPercent = ((TRY / openPNL.TRY) - 1) * 100;
            return percents;
        }
    }
}
