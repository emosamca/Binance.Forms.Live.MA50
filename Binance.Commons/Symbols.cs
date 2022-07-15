using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class Symbols
    {
        public string symbol { get; set; }
        public string status { get; set; }
        public string baseAsset { get; set; }
        public int baseAssetPrecision { get; set; }
        public string quoteAsset { get; set; }
        public int quoteAssetPrecision { get; set; }
        public int baseCommissionPrecision { get; set; }
        public int quoteCommissionPrecision { get; set; }
        public List<string> orderTypes { get; set; }
        public bool icebergAllowed { get; set; }
        public bool ocoAllowed { get; set; }
        public bool quoteOrderQtyMarketAllowed { get; set; }
        public bool isSpotTradingAllowed { get; set; }
        public bool isMarginTradingAllowed { get; set; }
        public List<Filters> filters { get; set; }
        public List<string> permissions { get; set; }

        public bool IsTrading
        {
            get
            {
                if (status == "TRADING")
                    return true;
                else
                    return false;
            }
        }

        public OrderDetails GetOrderDetails(double TotalAsset,double Price)
        {
            OrderDetails orderDetails = new OrderDetails();
            Filters pricefilter = filters.First(x => x.filterType == "PRICE_FILTER");
            int pricevirgulyeri = pricefilter.tickSize.LastIndexOf('1')-1;
            if (pricevirgulyeri == -1)
                pricevirgulyeri = 0;
            Filters adetfilters = filters.First(x => x.filterType == "LOT_SIZE");
            int adetvirgulyeri = adetfilters.stepSize.LastIndexOf('1')-1;
            if (adetvirgulyeri == -1)
                adetvirgulyeri = 0;
            Filters minnotional = filters.First(x => x.filterType == "MIN_NOTIONAL");
            double minmiktar = Convert.ToDouble(minnotional.minNotional.Replace('.',','));

            double alimmiktari = TotalAsset / Price;
            orderDetails.AlisAdediString = alimmiktari.ToString($"F{adetvirgulyeri}").Replace(',','.');
            orderDetails.AlisAdedi = Convert.ToDouble(orderDetails.AlisAdediString.Replace('.', ','));

            orderDetails.AlisFiyati = Price;
            orderDetails.AlisFiyatiString = Price.ToString($"F{pricevirgulyeri}").Replace(',', '.');

            if (TotalAsset < minmiktar)
                return null;

            return orderDetails;
        }
    }
}
