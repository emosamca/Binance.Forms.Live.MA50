using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    [Serializable]
    public class NewBuyTable
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string CoinName { get; set; }
        public long BuyDate { get; set; }
        public string BuyDatestr { get; set; }
        public double BuyPrice { get; set; }
        public double AlisMTS { get; set; }
        public double PriceNow { get; set; }
        public double ProfitNow { get; set; }
        public double SellPrice { get; set; }
        public double SellProfit { get; set; }
        public long SellDate { get; set; }
        public string SellDatestr { get; set; }
        public double SellCandleMTS { get; set; }
        public string SellReason { get; set; }
        public double MaxProfit { get; set; }
        public double MinProfit { get; set; }
        public int State { get; set; }
        public double TotalMoney { get; set; }
        public double TotalAdet { get; set; }

        public bool IsStop { get; set; }
        public double StopPercent { get; set; }

        public bool IsLocked { get; set; }
        public double stoplimit { get; set; }
        public double trackstop { get; set; }
        public double mumesik { get; set; }
        public double MTSOld { get; set; }
        public double MTSGuess { get; set; }
        public double BuyLimit { get; set; }
        public double CalculateProfitNow(double pricenow)
        {
            double profit = 0;
            profit = ((pricenow / BuyPrice) - 1) * 100;
            return profit;
        }
        public double SellLMTS()
        {
            //return 4 * Math.Pow(AlisMTS,2) - (8.1 * AlisMTS) + 4.105;
            //return 0.206 - (0.2 * AlisMTS);
            return 0.5015- (0.5 * AlisMTS);
        }

        public void Sell(double satisfiyati, string satissebebi, double satisLMTS)
        {
            State = 0;
            SellPrice = satisfiyati;
            SellReason = satissebebi;
            SellDate = DateHelper.GetCurrentTimeStam();
            SellDatestr = DateHelper.GetDateStrFromTimeStamp(SellDate);
            SellCandleMTS = satisLMTS;
            PriceNow = satisfiyati;
            ProfitNow = ((SellPrice / BuyPrice) - 1) * 100;
            SellProfit = ProfitNow;
        }
        public void Buy(double alisfiyati, double alisadedi, double alisMTS)
        {
            BuyDate = DateHelper.GetCurrentTimeStam();
            BuyPrice = alisfiyati;
            AlisMTS = alisMTS;
            PriceNow = alisfiyati;
            ProfitNow = 0;
            State = 1;
            TotalMoney = alisfiyati * alisadedi;
            TotalAdet = alisadedi;
            BuyDatestr = DateHelper.GetDateStrFromTimeStamp(BuyDate);
        }
        public void ProfitCalc()
        {
            ProfitNow= ((PriceNow / BuyPrice) - 1) * 100;
            if (ProfitNow > MaxProfit)
                MaxProfit = ProfitNow;
            if (ProfitNow < MinProfit)
                MinProfit = ProfitNow;
        }
        public override string ToString()
        {
            //command.Parameters.AddWithValue("@coinname", buyTable.CoinName);
            //command.Parameters.AddWithValue("@buydate", buyTable.BuyDate);
            //command.Parameters.AddWithValue("@buydatestr", buyTable.BuyDatestr);
            //command.Parameters.AddWithValue("@buyprice", buyTable.BuyPrice);
            //command.Parameters.AddWithValue("@alisMTS", buyTable.AlisMTS);
            //command.Parameters.AddWithValue("@pricenow", buyTable.PriceNow);
            //command.Parameters.AddWithValue("@profitnow", buyTable.ProfitNow);
            //command.Parameters.AddWithValue("@totalmoney", buyTable.TotalMoney);
            //command.Parameters.AddWithValue("@totaladet", buyTable.TotalAdet);
            //command.Parameters.AddWithValue("@state", buyTable.State);
            //command.Parameters.AddWithValue("@sellreason", buyTable.SellReason);
            //command.Parameters.AddWithValue("@sellmtscalc", buyTable.SellLMTS() + buyTable.AlisMTS);
            //command.Parameters.AddWithValue("@stop", buyTable.StopPercent);
            //command.Parameters.AddWithValue("@stop_active", buyTable.IsStop == true ? 1 : 0);
            //command.Parameters.AddWithValue("@trackstop", buyTable.trackstop);
            return CoinName +"\n"+ BuyDate + "\n" + BuyDatestr + "\n" + BuyPrice + "\n" + AlisMTS + "\n" + PriceNow + "\n" + ProfitNow + "\n" + TotalMoney + "\n" + TotalAdet + "\n" + State + "\n" + SellReason + "\n" + SellLMTS() + "\n" + StopPercent + "\n" + IsStop + "\n" + trackstop;
        }
    }
}
