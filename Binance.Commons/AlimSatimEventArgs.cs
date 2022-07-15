using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class AlimSatimEventArgs:EventArgs
    {
        public List<CoinList> AlimList { get; set; } = new List<CoinList>();
        public List<CoinList> SatimList { get; set; } = new List<CoinList>();
    }
}
