using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    [Serializable]
    public class Candlestick
    {
        public long OpenTime { get; set; }
        public string OpenTimeStr { get; set; }
        public string Name { get; set; }
        public float Open { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float? Close { get; set; }
        public float Volume { get; set; }
        public long CloseTime { get; set; }
        public float QuoteAssetVolume { get; set; }
        public int NumberOfTrades { get; set; }
        public float TakerBuyBaseAssetVolume { get; set; }
        public float TakerBuyQuoteAssetVolume { get; set; }
        public double? EMA50 { get; set; }
        public double? EMA200 { get; set; }
        public double? MACDLine { get; set; }
        public double? MACDHist { get; set; }
        public double? Signal { get; set; }
        public double? MidBand { get; set; }
        public double? UpperBand { get; set; }
        public double? LowerBand { get; set; }
        public double? CCI { get; set; }

        public double MTS { get; set; }
        public double? SMA9 { get; set; }
        public double? K { get; set; }
        public double? D { get; set; }
        public override string ToString()
        {
            String metin = "";
            metin += $"{Close}";

            //String metin = "Class verileri:\n";
            //TimeSpan time = TimeSpan.FromMilliseconds(OpenTime);
            //DateTime startdate = new DateTime(1970, 1, 1) + time;
            //metin += $"Open time:{startdate}\n";
            //metin += $"Open     :{Open}\n";
            //metin += $"Close    :{Close}\n";
            //metin += $"EMA50    :{EMA50}\n";
            //metin += $"EMA200   :{EMA200}\n";
            //metin += $"MACD hist:{MACDHist}\n";
            //metin += $"BB Upper :{UpperBand}\n";
            //metin += $"BB Lower :{LowerBand}\n";
            //metin += "--------------------------";
            return metin;
        }
        public double GetRegresyon()
        {
            double reg = (Open + (double)Close + Low + High) / 4;
            return reg;
        }
    }
}
