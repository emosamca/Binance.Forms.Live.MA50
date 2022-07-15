using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class UserSocketConnectionMessageEventArgs : EventArgs
    {
        public userstreams stream { get; set; }
    }
}
