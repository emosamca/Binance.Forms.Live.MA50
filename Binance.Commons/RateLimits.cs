using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class RateLimits
    {
        public string rateLimitType { get; set; }
        public string interval { get; set; }
        public int intervalNum { get; set; }
        public int limit { get; set; }
    }
}
