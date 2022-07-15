using Binance.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Indicator
{
    // Stokastik.Bu formul ile hesaplanır: 100 * (close - lowest(low, length)) / (highest(high, length) - lowest(low, length))
    // k: mavi d:turuncu
    //  periodK = input(14, title= "K", minval= 1)
    //  periodD = input(3, title= "D", minval= 1) // smoothD
    //  smoothK = input(3, title= "Smooth", minval= 1)
    //  k = sma(stoch(close, high, low, periodK), smoothK)
    //  d = sma(k, periodD)

    public class Stochastic : IndicatorCalculatorBase<StochSerie>
    {
        protected override List<Candlestick> OhlcList { get; set; }
        protected int Period = 14;
        protected int PeriodD = 3;
        protected int SmoothK = 1;

        public Stochastic()
        {

        }
        public Stochastic(int period, int periodD, int smoothK)
        {
            this.Period = period;
            this.PeriodD = periodD;
            this.SmoothK = smoothK;
        }
        public override StochSerie Calculate()
        {
            StochSerie stochSerie = new StochSerie();
            List<Candlestick> forK = new List<Candlestick>();
            for (int i = 0; i < OhlcList.Count; i++)
            {

                if (i >= Period - 1)
                {
                    var ohlc = new Candlestick();
                    double low = lowest(i);
                    double high = highest(i);
                    ohlc.Close = (float)(100 * ((double)OhlcList[i].Close - low) / (high - low));
                    forK.Add(ohlc);
                }
                else
                {
                    var ohlc = new Candlestick() { Close = 0 };
                    forK.Add(ohlc);
                }
            }
            SMA sma = new SMA(SmoothK);
            sma.Load(forK);
            var sonuc=sma.Calculate();
            stochSerie.k = sonuc.Values.DeepClone();
            forK = new List<Candlestick>();
            foreach (var item in sonuc.Values)
            {
                var candle = new Candlestick();
                candle.Close = item == null ? 0 : (float?)item;
                forK.Add(candle);
            }
            sma = new SMA(PeriodD);
            sma.Load(forK);
            sonuc = sma.Calculate();
            stochSerie.d = sonuc.Values.DeepClone();
            return stochSerie;

        }
        public double lowest(int i)
        {
            double low = double.MaxValue;
            for (int k = i - 13; k <= i; k++)
            {
                if (OhlcList[k].Low < low)
                    low = OhlcList[k].Low;
            }
            return low;
        }
        public double highest(int i)
        {
            double high = double.MinValue;
            for (int k = i - 13; k <= i; k++)
            {
                if (OhlcList[k].High > high)
                    high = OhlcList[k].High;
            }
            return high;
        }
    }
}
