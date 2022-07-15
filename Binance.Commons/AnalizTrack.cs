using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    [Serializable]
    public class AnalizTrack
    {
        public long Id { get; set; }
        public long Date { get; set; }
        public string Datestr { get; set; }
        public string Sure { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string State { get; set; }
        public double SellPrice { get; set; }
        public double Profit { get; set; }
        public double MaxPrice { get; set; }
        public double MaxProfit { get; set; }
        public double CurrentPrice { get; set; }
        public double CurrentProfit { get; set; }
        public double Volort { get; set; }
        public double AlimVolort { get; set; }
        public bool Changed { get; set; }
        public int AlimType { get; set; }
        public long Selldate { get; set; }
        public string Selldatestr { get; set; }
        public string AlimNotu { get; set; }
        public string Levels { get; set; }
          public override string ToString()
        {
            return $"{Id} {Date} {Datestr} {Sure} {Name} {Price} {State} {SellPrice} {AlimType} {AlimNotu} {Volort} {AlimVolort}";

        }  
        public void CalculateProfit()
        {
            Profit = ((SellPrice / Price) - 1) * 100;
        }
        public void CalculateMaxProfit()
        {
            MaxProfit = ((MaxPrice / Price) - 1) * 100;
        }
        public void CalculateCurrentProfit()
        {
            CurrentProfit = ((CurrentPrice / Price) - 1) * 100;
        }

    }
}
