using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class futuredatauserstream
    {
        public string e { get; set; }
        public long E { get; set; }

        public List<futureOrderDetails> o { get; set; }
    }
}
