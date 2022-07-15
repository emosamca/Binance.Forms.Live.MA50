using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
   public class OrderDetails
    {
        public double AlisFiyati { get; set; }
        public double AlisAdedi { get; set; }
        public string AlisFiyatiString { get; set; }
        public string AlisAdediString { get; set; }
        public double TotalFiyat
        {
            get 
            {
                return AlisAdedi * AlisFiyati;
            }
        }
    }
}
