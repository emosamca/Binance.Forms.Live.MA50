using Binance.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Indicator
{
    public abstract class IndicatorCalculatorBase<T>
    {
        protected abstract List<Candlestick> OhlcList { get; set; }


        public virtual void Load(List<Candlestick> ohlcList)
        {
            this.OhlcList = ohlcList.DeepClone();
        }

        public abstract T Calculate();
    }
}
