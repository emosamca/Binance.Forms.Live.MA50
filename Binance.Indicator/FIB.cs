using Binance.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Indicator
{
    public class FIB: IndicatorCalculatorBase<FibValues>
    {
        protected override List<Candlestick> OhlcList { get ; set; }
        protected int candleCount=50;
        public FIB(int CandleCount)
        {
            this.candleCount = CandleCount;
        }

        public override FibValues Calculate()
        {
            if(OhlcList.Count< candleCount)
                return new FibValues();
            FibValues fibValues;
            bool tekrar = false;
            do
            {
                tekrar = false;
                fibValues = new FibValues();
                int len = OhlcList.Count;
                int start = len - candleCount - 1;
                if (start < 0)
                    return new FibValues();
                int finish = len - 1;
                int maxind = start;
                int minind = start;
                float maxval = (float)OhlcList[start].High;
                float minval = (float)OhlcList[start].Low;
                for (int i = start + 1; i < finish; i++)
                {
                    if ((float)OhlcList[i].High >= maxval)
                    {
                        maxind = i;
                        maxval = (float)OhlcList[i].High;
                    }
                    if ((float)OhlcList[i].Low <= minval)
                    {
                        minind = i;
                        minval = (float)OhlcList[i].Low;
                    }
                }
                float delta = Math.Abs(maxval - minval);
                if (maxind < minind)
                {
                    fibValues.fib0 = minval;
                    fibValues.fib236 = minval + (delta * (float)0.236);
                    fibValues.fib382 = minval + (delta * (float)0.382);
                    fibValues.fib50 = minval + (delta * (float)0.5);
                    fibValues.fib618 = minval + (delta * (float)0.618);
                    fibValues.fib786 = minval + (delta * (float)0.786);
                    fibValues.fib100 = maxval;
                    //if (OhlcList[len - 2].Close < fibValues.fib236)
                    //{
                    //    candleCount = len - maxind - 2;
                    //    tekrar = true;
                    //}
                }
                if (maxind >= minind)
                {
                    fibValues.fib0 = maxval;
                    fibValues.fib236 = maxval - (delta * (float)0.236);
                    fibValues.fib382 = maxval - (delta * (float)0.382);
                    fibValues.fib50 = maxval - (delta * (float)0.5);
                    fibValues.fib618 = maxval - (delta * (float)0.618);
                    fibValues.fib786 = maxval - (delta * (float)0.786);
                    fibValues.fib100 = minval;
                    //if (OhlcList[len - 2].Close > fibValues.fib236)
                    //{
                    //    candleCount = len - minind - 2;
                    //    tekrar = true;
                    //}
                }
            } while (tekrar);
            return fibValues;
        }
    }
}
