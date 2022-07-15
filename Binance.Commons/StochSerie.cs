using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class StochSerie:IIndicatorSerie
    {
        public List<double?> k { get; set; }
        public List<double?> d { get; set; }

        public StochSerie()
        {
            this.k = new List<double?>();
            this.d = new List<double?>();
        }
    }
}
