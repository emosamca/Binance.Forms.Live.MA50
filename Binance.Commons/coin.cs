using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    [Serializable]
    public class coin
    {
        public long id { get; set; }
        public string name { get; set; }
        public long candlestart { get; set; }
        public double atrval { get; set; }
        public bool changed { get; set; }
        public double MTS { get; set; }
        public double MTSGuess { get; set; }
        public double MTSOld { get; set; }
        public double last24hoursprofit { get; set; }
        public double lastDayProfit { get; set; }
        public bool InAnalyze { get; set; }
        public bool active { get; set; }
        public coin()
        {
            changed = false;
        }
    }
}
