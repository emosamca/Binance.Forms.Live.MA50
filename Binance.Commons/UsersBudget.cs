using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public class UsersBudget
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public double StartBudget { get; set; }
        public double TargetBudget { get; set; }
        public int TradeMax { get; set; }
        public int TradeNow { get; set; }
        public double RemainingMoney { get; set; }
        public double Withdrawmoney { get; set; }
        public double DepositMoney { get; set; }
        public int TradeEnable { get; set; }
        public int TempTradeDisable { get; set; }
        public int IsAlive { get; set; }
        public string HtmlHeader { get; set; }
        public string Seviyeler { get; set; }
        public string BudgetName { get; set; }
        public int TradeTytpe { get; set; }
        public double NetWorth { get; set; }
        public double realstartbudget { get; set; }
        public double totalmoney { get; set; }
        public double realtotalmoney { get; set; }
        public double profit { get; set; }
        public double realprofit { get; set; }
        public double bnbbalance { get; set; }
        public double bnbbalanceusdt { get; set; }
        public bool autobotenable { get; set; }
        public bool bottradeenable { get; set; }

        public bool stoplossenable { get; set; }
        public double stoplossvalue { get; set; }
        public void CalculateProfits(double alimdakilerintoplami)
        {
            totalmoney = RemainingMoney + alimdakilerintoplami;
            realtotalmoney = totalmoney - TargetBudget;
            realstartbudget = StartBudget + DepositMoney - Withdrawmoney;
            if (realstartbudget == 0)
                realstartbudget = 0.1;
            profit = ((totalmoney / realstartbudget) - 1) * 100;
            realprofit= (((totalmoney-TargetBudget) / realstartbudget) - 1) * 100;
        }
        //        ALTER TABLE `usersbudget` 
        //ADD COLUMN `realstartbudget` double NULL DEFAULT '0',
        //add column `totalmoney` double NULL DEFAULT '0',
        //add column `profit` double NULL DEFAULT '0',
        //add column `realprofit` double NULL DEFAULT '0'
        //realtotalmoney
        /// <summary>
        /// Belirtilen seviyeyi döndürür
        /// </summary>
        public int GetNewSeviye(double fiyat,double currentprice)
        {
            String[] seviye_str= Seviyeler.Split(',');
            int donusseviye = 0;
            int yeniseviye = 0;
            foreach (var item in seviye_str)
            {
                double deger = 0;
                deger = Convert.ToDouble(item.Replace('.',','));
                double yuzde = (100 + deger) / 100;
                if (donusseviye==0)
                {
                    if (fiyat * yuzde > currentprice)
                        return -1;
                }
                if (currentprice > fiyat * yuzde)
                    yeniseviye = donusseviye;
                donusseviye++;
            }

            return yeniseviye;
        }
        public int GetSeviyeCount()
        {
            String[] seviye_str = Seviyeler.Split(',');
            return seviye_str.Length;
        }

        public void CoinSelled(double cummulativeQuoteQty)
        {
            RemainingMoney += cummulativeQuoteQty;
            TradeNow--;
            TargetBudget += 0;// cummulativeQuoteQty * (0.075 / 100);
        }

        public void CoinBuyed(double cummulativeQuoteQty)
        {
            RemainingMoney -= cummulativeQuoteQty;
            TradeNow++;
            TargetBudget += 0;// cummulativeQuoteQty * (0.075 / 100);
        }
    }
}
