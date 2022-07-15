using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class WorkerReturnObj
    {
        public AnalizTrack Eklenecek { get; set; }
        public List<AnalizTrack> Guncellenecek { get; set; } = new List<AnalizTrack>();
        public double SonMumBaski { get; set; }
        public double Son6SaatlikBaski { get; set; }
        public double GunlukBaski { get; set; }
        public string GoldenCrosses { get; set; }
        public string DeathCrosses { get; set; }
        public Izleme IzlemeEklenecek { get; set; }
    }
}
