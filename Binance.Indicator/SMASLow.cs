using Binance.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Indicator
{
    public class SMASLow : IndicatorCalculatorBase<SMASValues>
    {
        protected override List<Candlestick> OhlcList { get; set; }

        public int offset { get; set; }
        //public bool UseCurrent { get; set; }
        public bool DiptenAlAktif { get; set; }

        public double MumEsik { get; set; }
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
            //offset = 0;
            SMASValues smaSerie = new SMASValues();

            smaSerie.AnalizValueBuy = false;
            double low;
            var sonmum = OhlcList[OhlcList.Count - 1 + offset];
            var sonmumoncekimum= OhlcList[OhlcList.Count - 1 + offset-1];
            var sonmumkapanis = (double)sonmum.Close;
            var sonmumoncekimumkapanis= (double)sonmumoncekimum.Close;
            MumEsik = sonmumkapanis / sonmumoncekimumkapanis;
            //if (!UseCurrent)
            //low = (double)sonmum.Low;
            //else
            low = (double)sonmum.Close;


            smaSerie.Last = GetAverage(1, offset);
            smaSerie.Last2 = GetAverage(2, offset);
            smaSerie.Last4 = GetAverage(4, offset);
            smaSerie.Last8 = GetAverage(8, offset);
            smaSerie.Last16 = GetAverage(16, offset);
            smaSerie.Last32 = GetAverage(32, offset);
            smaSerie.Last64 = GetAverage(64, offset);
            smaSerie.Last96 = GetAverage(96, offset);


            smaSerie.LMTS = (0.125 * (low / smaSerie.Last)) + (0.125 * (low / smaSerie.Last2)) + (0.125 * (low / smaSerie.Last4)) + (0.125 * (low / smaSerie.Last8)) + (0.125 * (low / smaSerie.Last16)) + (0.125 * (low / smaSerie.Last32)) + (0.125 * (low / smaSerie.Last64)) + (0.125 * (low / smaSerie.Last96));

            double mean = smaSerie.Last16;
            double std = 0.0;
            int lastindex = OhlcList.Count - 1;
            for (int i = 1; i < 17; i++)
            {
                std += Math.Pow(mean - (double)OhlcList[lastindex - i + offset].Close, 2);
            }
            std = Math.Sqrt( std / 15.0);
            smaSerie.Esik = mean - std;

            if ((double)sonmum.Close < smaSerie.Esik)
                smaSerie.CloseUygun = true;
            else
                smaSerie.CloseUygun = false;
            if ((double)sonmum.Close < (double)sonmumoncekimum.Close)
                smaSerie.BirOncekiCloseUygun = true;
            else
                smaSerie.BirOncekiCloseUygun = false;
            var lastdaycandle = OhlcList[OhlcList.Count - 1 + offset - 96];
            smaSerie.LastDailyProfit = (((double)sonmum.Close / lastdaycandle.Open) - 1) * 100;
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

