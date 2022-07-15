using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class SingleDoubleSerie : IIndicatorSerie
    {
        public List<double?> Values { get; set; }

        public SingleDoubleSerie()
        {
            Values = new List<double?>();
        }
    }
}
