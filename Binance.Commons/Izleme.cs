using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class Izleme
    {
        public long Id { get; set; }
        public string Symbol { get; set; }
        public string Interval { get; set; }
        public double Volort { get; set; }
        public double Alimort { get; set; }
        public long Date { get; set; }
        public string Datestr { get; set; }
        public string Mesaj { get; set; }
    }
}
