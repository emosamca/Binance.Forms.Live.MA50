using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public static class GlobalVars
    {
        public static bool Istest { get; set; }

        public static bool btc50up { get; set; }
        public static bool btc20up { get; set; }

        public static int CandleCount { get; set; }

        public static bool BotFullStop { get; set;}
        public static bool OnlyBuyStop { get; set; }
        public static bool OncekiBotFullStop { get; set; }
        public static bool OncekiOnlyBuyStop { get; set; }

    }
}
