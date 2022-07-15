using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class User
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Apikey { get; set; }
        public string Apisecretkey { get; set; }
        public long Userid { get; set; }
        public long Groupid { get; set; }
        public int Tradecount { get; set; }
        public bool Pnlsend { get; set; }
        public bool Ordersend { get; set; }
        public bool Tradeenable { get; set; }
        public string Htmlheader { get; set; }
        public int autobot { get; set; }
        public User() // long id, string username, string apikey, string apisecretkey, long userid, long groupid, int tradecount, bool pnlsend, bool ordersend, bool tradeenable, string header)
        {
            //Id = id;
            //UserName = username;
            //Apikey = apikey;
            //Apisecretkey = apisecretkey;
            //Userid = userid;
            //Groupid = groupid;
            //Tradecount = tradecount;

        }
    }
}
