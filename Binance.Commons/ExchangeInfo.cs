using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class ExchangeInfo
    {
        public string timezone { get; set; }
        public long serverTime { get; set; }
        public List<RateLimits> rateLimits { get; set; }
        public List<Symbols> symbols { get; set; }

    }
}
