using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class UsersGetInfo
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Symbol { get; set; }
        public string Sure { get; set; }
        public long Date { get; set; }
        public int State { get; set; }
        public string Datestr { get; set; }
    }
}
