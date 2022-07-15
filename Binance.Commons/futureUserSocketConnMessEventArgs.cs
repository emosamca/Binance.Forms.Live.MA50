using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class futureUserSocketConnMessEventArgs:EventArgs
    {
        public string stream { get; set; }
    }
}
