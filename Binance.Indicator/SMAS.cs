using Binance.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Indicator
{
    public class SMAS : IndicatorCalculatorBase<SMASValues>
    {
        protected override List<Candlestick> OhlcList { get; set; }

        public int offset { get; set; }
        public bool DiptenAlAktif { get; set; }
        /// <summary>
        /// Daily Closing Prices: 11,12,13,14,15,16,17 
        /// First day of 5-day SMA: (11 + 12 + 13 + 14 + 15) / 5 = 13
        /// Second day of 5-day SMA: (12 + 13 + 14 + 15 + 16) / 5 = 14
        /// Third day of 5-day SMA: (13 + 14 + 15 + 16 + 17) / 5 = 15 
        /// </summary>
        /// <see cref="http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_averages"/>
        /// <returns></returns>
        public override SMASValues Calculate()
        {
            SMASValues smaSerie = new SMASValues();

            smaSerie.AnalizValueBuy = false;
            double high;
            high = (double)OhlcList[OhlcList.Count-1+offset].High;


            smaSerie.Last = GetAverage(1, offset);
            smaSerie.Last2 = GetAverage(2, offset);
            smaSerie.Last4 = GetAverage(4, offset);
            smaSerie.Last8 = GetAverage(8, offset);
            smaSerie.Last16 = GetAverage(16, offset);
            smaSerie.Last32 = GetAverage(32, offset);
            smaSerie.Last64 = GetAverage(64, offset);
            smaSerie.Last96 = GetAverage(96, offset);

            smaSerie.Last2old = GetAverage(2, offset-1);
            smaSerie.Last16old = GetAverage(16, offset-1);
            smaSerie.Last96old = GetAverage(96, offset-1);

            smaSerie.AnalizValue = (0.125 * (high / smaSerie.Last)) + (0.125 * (high / smaSerie.Last2)) + (0.125 * (high / smaSerie.Last4)) + (0.125 * (high / smaSerie.Last8)) + (0.125 * (high / smaSerie.Last16)) + (0.125 * (high / smaSerie.Last32)) + (0.125 * (high / smaSerie.Last64)) + (0.125 * (high / smaSerie.Last96));
            //smaSerie.AnalizValue = (0.35 * (high / smaSerie.Last)) + (0.25 * (high / smaSerie.Last2)) + (0.15 * (high / smaSerie.Last4)) + (0.10 * (high / smaSerie.Last8)) + (0.075 * (high / smaSerie.Last16)) + (0.025 * (high / smaSerie.Last32)) + (0.025 * (high / smaSerie.Last64)) + (0.025 * (high / smaSerie.Last96));

            //if (DiptenAlAktif)
            //{
            //    high = (double)OhlcList[OhlcList.Count - 2].High;


            //    smaSerie.Last = GetAverage(1, 1);
            //    smaSerie.Last2 = GetAverage(2, 1);
            //    smaSerie.Last4 = GetAverage(4, 1);
            //    smaSerie.Last8 = GetAverage(8, 1);
            //    smaSerie.Last16 = GetAverage(16, 1);
            //    smaSerie.Last32 = GetAverage(32, 1);
            //    smaSerie.Last64 = GetAverage(64, 1);
            //    smaSerie.Last96 = GetAverage(96, 1);

            //    smaSerie.AnalizValue2 = (0.125 * (high / smaSerie.Last)) + (0.125 * (high / smaSerie.Last2)) + (0.125 * (high / smaSerie.Last4)) + (0.125 * (high / smaSerie.Last8)) + (0.125 * (high / smaSerie.Last16)) + (0.125 * (high / smaSerie.Last32)) + (0.125 * (high / smaSerie.Last64)) + (0.125 * (high / smaSerie.Last96));

            //    high = (double)OhlcList[OhlcList.Count - 3].High;


            //    smaSerie.Last = GetAverage(1, 2);
            //    smaSerie.Last2 = GetAverage(2, 2);
            //    smaSerie.Last4 = GetAverage(4, 2);
            //    smaSerie.Last8 = GetAverage(8, 2);
            //    smaSerie.Last16 = GetAverage(16, 2);
            //    smaSerie.Last32 = GetAverage(32, 2);
            //    smaSerie.Last64 = GetAverage(64, 2);
            //    smaSerie.Last96 = GetAverage(96, 2);

            //    smaSerie.AnalizValue3 = (0.125 * (high / smaSerie.Last)) + (0.125 * (high / smaSerie.Last2)) + (0.125 * (high / smaSerie.Last4)) + (0.125 * (high / smaSerie.Last8)) + (0.125 * (high / smaSerie.Last16)) + (0.125 * (high / smaSerie.Last32)) + (0.125 * (high / smaSerie.Last64)) + (0.125 * (high / smaSerie.Last96));

            //    if (smaSerie.AnalizValue3 < 0.97 && smaSerie.AnalizValue2 > 0.97)
            //        smaSerie.AnalizValueBuy = true;
            //}
            return smaSerie;
        }

        private double GetAverage(int count, int offset)
        {
            int lastindex = OhlcList.Count - 1;
            double sum = 0;
            for (int y = 1; y < count + 1; y++)
            {
                sum += (double)OhlcList[lastindex - y + offset].Close;
            }
            return sum / count;
        }
    }
}
