using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class TelegramMessageEventArgs:EventArgs
    {
        public string Messages { get; set; }
        public long UserId { get; set; }
        public long ChatId { get; set; }
    }
}
