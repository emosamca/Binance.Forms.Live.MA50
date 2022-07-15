using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class FibValues : IIndicatorSerie
    {
        public float fib0 { get; set; }
        public float fib236 { get; set; }
        public float fib382 { get; set; }
        public float fib50 { get; set; }
        public float fib618 { get; set; }
        public float fib786 { get; set; }
        public float fib100 { get; set; }
        public string Levels { get; set; }
        public bool CoinIsBTC { get; set; }
        public FibValues()
        {

        }

        public string GetStopLoss(double Price)
        {
            float deger = 0;
            if (fib0 > fib100)
            {
                if (Price > fib100 && Price < fib786)
                    deger= fib100;
                else if (Price >= fib786 && Price < fib618)
                    deger = fib786;
                else if (Price >= fib618 && Price < fib50)
                    deger = fib618;
                else if (Price >= fib50 && Price < fib382)
                    deger = fib50;
                else if (Price >= fib382 && Price < fib236)
                    deger = fib382;
                else if (Price >= fib236 && Price < fib0)
                    deger = fib236;
            }
            else if(fib100>fib0)
            {
                if (Price > fib0 && Price < fib236)
                    deger = fib0;
                else if (Price >= fib236 && Price < fib382)
                    deger = fib236;
                else if (Price >= fib382 && Price < fib50)
                    deger = fib382;
                else if (Price >= fib50 && Price < fib618)
                    deger = fib50;
                else if (Price >= fib618 && Price < fib786)
                    deger = fib618;
                else if (Price >= fib786 && Price < fib100)
                    deger = fib786;
            }
            return ToBinString(deger);
        }
        public string GetSellLevels(double Price)
        {
            if (fib0 > fib100)
            {
                if (Price > fib100 && Price < fib786)
                    Levels = ToBinString(fib786) + "-" + ToBinString(fib618) + "-" + ToBinString(fib50) + "-" + ToBinString(fib382) + "-" + ToBinString(fib236) + "-" + ToBinString(fib0);
                else if (Price >= fib786 && Price < fib618)
                    Levels = ToBinString(fib618) + "-" + ToBinString(fib50) + "-" + ToBinString(fib382) + "-" + ToBinString(fib236) + "-" + ToBinString(fib0);
                else if (Price >= fib618 && Price < fib50)
                    Levels = ToBinString(fib50) + "-" + ToBinString(fib382) + "-" + ToBinString(fib236) + "-" + ToBinString(fib0);
                else if (Price >= fib50 && Price < fib382)
                    Levels = ToBinString(fib382) + "-" + ToBinString(fib236) + "-" + ToBinString(fib0);
                else if (Price >= fib382 && Price < fib236)
                    Levels =ToBinString(fib236) + "-" + ToBinString(fib0);
                else if (Price >= fib236 && Price < fib0)
                    Levels =ToBinString(fib0);
            }
            else if (fib100 > fib0)
            {
                if (Price > fib0 && Price < fib236)
                    Levels = ToBinString(fib236) + "-" + ToBinString(fib382) + "-" + ToBinString(fib50) + "-" + ToBinString(fib618) + "-" + ToBinString(fib786) + "-" + ToBinString(fib100);
                else if (Price >= fib236 && Price < fib382)
                    Levels =  ToBinString(fib382) + "-" + ToBinString(fib50) + "-" + ToBinString(fib618) + "-" + ToBinString(fib786) + "-" + ToBinString(fib100);
                else if (Price >= fib382 && Price < fib50)
                    Levels =  ToBinString(fib50) + "-" + ToBinString(fib618) + "-" + ToBinString(fib786) + "-" + ToBinString(fib100);
                else if (Price >= fib50 && Price < fib618)
                    Levels = ToBinString(fib618) + "-" + ToBinString(fib786) + "-" + ToBinString(fib100);
                else if (Price >= fib618 && Price < fib786)
                    Levels = ToBinString(fib786) + "-" + ToBinString(fib100);
                else if (Price >= fib786 && Price < fib100)
                    Levels = ToBinString(fib100);
            }
            return Levels;
        }
        public string ToBinString(double deger)
        {
            var str = deger.ToString("F8");
            string degerim = "";
            if (str.StartsWith("0,") && CoinIsBTC)
            {
                bool basl = true;
                for (int k = 0; k < str.Length; k++)
                {
                    if ((str[k] == '0' || str[k] == ',') && basl)
                        continue;
                    basl = false;
                    degerim += str[k];
                }

            }
            else
                degerim = str;
            return degerim;
        }
    }
}
