using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Commons;
using Binance.Connections;
using Binance.Indicator;
using Binance.Mysql;

namespace Binance.Forms
{
    public class CCTSHelper
    {
        public MysqlClass mysqlclass;
        public Services services;
        public int mumsayisi { get; set; }
        public bool alalimmi { get; set; }
        public double LMTSlimit { get; set; }
        public double ortalama { get; set; }
        public double MTSoldortalama { get; set; }
        public double cctsma20 { get; set; }
        public double max { get; set; }
        public double min { get; set; }
        public double stdsapm { get; set; }
        public int mincoincount { get; set; }
        public List<CctsCandle> cctscandles { get; set; }
        public List<Candlestick> btccandlestick { get; set; }

        public double sondortmumortalama { get; set; }
        public  CCTSHelper(MysqlClass mysqlClass, Services binanceservice)
        {
            mysqlclass = mysqlClass;
            services = binanceservice;
        }

        public void Calculate(List<NewBuyTable> tumliste, List<NewBuyTable> alimlistesi, bool onlycalculateLMTS)
        {
            // mum sayısı hesaplanıyor
            var tummumlarinmumesikortalamasi = tumliste.Average(x => x.mumesik);
            Debug.WriteLine($"Tüm mumların eşik ortalaması: {tummumlarinmumesikortalamasi}");
            if (tummumlarinmumesikortalamasi >= 1)
                mumsayisi = 16;
            else if (tummumlarinmumesikortalamasi <= 0.9)
                mumsayisi = 1;
            else
                mumsayisi = Convert.ToInt32(Math.Ceiling((150 * tummumlarinmumesikortalamasi) - 134));
            Debug.WriteLine($"Hesaplanan mum sayısı: {mumsayisi}");
            if (mumsayisi > 16)
                mumsayisi = 16;
            Debug.WriteLine($"Düzeltilen mum sayısı: {mumsayisi}");

            // alım için analiz değilse kaydedilmiyor
            if (!onlycalculateLMTS)
            {
                GlobalVars.CandleCount = mumsayisi;
                var btcstates = mysqlclass.GetBTCma50states();
                btcstates.candlecount = mumsayisi;
                mysqlclass.UpdateBTCandETHstatesCandleCount(btcstates);
                mysqlclass.DeleteBannedList();
            }
            // alim listesindekilern LMTS ortalaması
            ortalama = alimlistesi.Average(x => x.AlisMTS);
            mincoincount= alimlistesi.Count(x => x.AlisMTS < 0.995);

            // alım için analiz değilse kaydedilmiyor
            if (!onlycalculateLMTS)
            {
                CctsCandle cctsCandle = new CctsCandle();
                cctsCandle.ccts = ortalama;
                cctsCandle.candleopentime = DateHelper.GetCurrentTimeStam();
                cctsCandle.mincoincount = mincoincount;
                mysqlclass.SaveCCTScandle(cctsCandle);


            }
            cctscandles = mysqlclass.GetCCTScandle();
            alalimmi = true;
            if (cctscandles.Count > 96 && !onlycalculateLMTS)
            {
                var getfazlalik = cctscandles.OrderBy(x => x.candleopentime).Take(cctscandles.Count - 96).ToList();
                foreach (var item in getfazlalik)
                {
                    mysqlclass.DeleteCCTScandle(item);
                }
                cctscandles = mysqlclass.GetCCTScandle();
            }

            if (onlycalculateLMTS)
                cctscandles.Add(new CctsCandle() { id = 0, ccts = ortalama, candleopentime = DateHelper.GetCurrentTimeStam(), mincoincount=mincoincount });

            if (cctscandles.Count >= 20)
            {
                var asilliste = cctscandles.OrderByDescending(x => x.candleopentime).Take(20).ToList();
                bool analiz1 = Analiz1(onlycalculateLMTS);
                if (analiz1)
                {
                    bool analiz2 = Analiz2(onlycalculateLMTS);
                    alalimmi = analiz1 && analiz2;
                }
                else
                    alalimmi = false;                
                cctsma20 = asilliste.Average(x => x.ccts);
            }
            else if (cctscandles.Count < 20)
            {
                alalimmi = true;
            }

            MTSoldortalama = alimlistesi.Average(x => x.MTSOld);

            max = alimlistesi.Max(x => x.AlisMTS);
            Debug.WriteLine($"Max lmts : {max}");
            min = alimlistesi.OrderBy(x => x.AlisMTS).Take(3).ToList().Average(y => y.AlisMTS);
            Debug.WriteLine($"Min lmts : {min}");
            stdsapm = 0;
            foreach (var item in alimlistesi)
            {
                stdsapm += Math.Pow(ortalama - item.AlisMTS, 2);
            }
            stdsapm = Math.Sqrt(stdsapm / (alimlistesi.Count - 1));
            double riskfaktor = mysqlclass.GetRiskFaktorValue();
            if (ortalama <= 0.93)
                LMTSlimit = 0.88;
            else
                LMTSlimit = ortalama - (stdsapm + (riskfaktor * min) * (ortalama - min));

            Debug.WriteLine($"Kontrol lmts : {LMTSlimit}");
        }
        private bool AlimDurumuHesapla(List<CctsCandle> asilliste)
        {
            double ma20 = asilliste.Average(x => x.ccts);
            double sonccts = asilliste.First().ccts;
            Debug.WriteLine($"{DateTime.Now.ToString()}-CCTS-{sonccts}");
            if (sonccts > 1.015)
                return false;
            else if (sonccts <= ma20)
                return true;
            else if (sonccts > ma20)
            {
                //CCTSort>ma4>ma8>ma12>ma16>ma20 
                double ma4 = asilliste.Take(4).Average(x => x.ccts);
                double ma8 = asilliste.Take(8).Average(x => x.ccts);
                double ma12 = asilliste.Take(12).Average(x => x.ccts);
                double ma16 = asilliste.Take(16).Average(x => x.ccts);
                if (sonccts > ma4 && ma4 > ma8 && ma8 > ma12 && ma12 > ma16 && ma16 > ma20)
                    return true;
                else
                    return false;
            }
            return true;
        }

        public bool Analiz1(bool onlycalculateLMTS)
        {
            //btc bolinger üst bandından büyükmü?
            //1
            btccandlestick = services.GetKlines("BTCUSDT", "15m", 100);
            if(!onlycalculateLMTS)
                if (btccandlestick.Last().CloseTime > DateHelper.GetCurrentTimeStam())
                    btccandlestick.RemoveAt(btccandlestick.Count - 1);
            var btcsonmum = btccandlestick[btccandlestick.Count - 1];



            BollingerBand btc_bb = new BollingerBand();
            btc_bb.Load(btccandlestick);
            var btc_bbsonuc = btc_bb.Calculate();
            Debug.WriteLine($"Analiz1-BB#1");
            if ((double)btcsonmum.Close > (double)btc_bbsonuc.UpperBand[btc_bbsonuc.UpperBand.Count - 1])
                return false;
            
            SMASLow sMASLow = new SMASLow();
            sMASLow.Load(btccandlestick);
            var btc_cctssonuc = sMASLow.Calculate();

            //2
            Debug.WriteLine($"Analiz1-LMTS#2");
            if (btc_cctssonuc.LMTS > 1.02)
                return false;

            //3
            // ortalama = tüm coinlerin (analize dahil) ccts ortalaması
            Debug.WriteLine($"Analiz1-LMTSort#3");
            if (ortalama > 1.02)
                return false;

            //4
            Debug.WriteLine($"Analiz1-LMTSBB#4");
            BollingerBandSerie cctsbolsonuc =null;
            if (cctscandles.Count > 24)
            {
                cctscandles = cctscandles.OrderByDescending(x => x.candleopentime).Take(25).OrderBy(x => x.candleopentime).ToList();
                List<Candlestick> cctscandlestick = new List<Candlestick>();
                foreach (var item in cctscandles)
                {
                    cctscandlestick.Add(new Candlestick() { Close = (float)item.ccts });
                }
                BollingerBand cctsbol = new BollingerBand();
                cctsbol.Load(cctscandlestick);
                cctsbolsonuc = cctsbol.Calculate();

                if (ortalama > (double)cctsbolsonuc.UpperBand[cctsbolsonuc.UpperBand.Count - 1] && ortalama < 1)
                    return false;

                //5
                Debug.WriteLine($"Analiz1-LMTSBB#5");
                if (ortalama > (double)cctsbolsonuc.MidBand[cctsbolsonuc.MidBand.Count - 1] && ortalama < 0.99)
                    return false;

                //6
                Debug.WriteLine($"Analiz1-LMTSBB#6");
                if (ortalama > (double)cctsbolsonuc.MidBand[cctsbolsonuc.MidBand.Count - 1] && btc_cctssonuc.LMTS > 1.015)
                    return false;
            }
            //7
            Debug.WriteLine($"Analiz1-LMTS#7");
            if (cctscandles.Count > 21)
            {
                cctscandles = cctscandles.OrderByDescending(x => x.candleopentime).ToList();
                var mincoincountMA20 = cctscandles.Take(20).Average(x => x.mincoincount);
                var mincoincountMA20onceki = cctscandles.Skip(1).Take(20).Average(x => x.mincoincount);
                if (mincoincount > 130 && mincoincount <= 180 && mincoincountMA20 < mincoincountMA20onceki)
                    return false;
            }
            Debug.WriteLine($"Analiz1-son");
            return true;
        }
        public bool Analiz2(bool onlycalculateLMTS)
        {
            //1
            sondortmumortalama = 0;
            for (int i = 1; i < 5; i++)
            {
                sondortmumortalama += ((double)btccandlestick[btccandlestick.Count - i].Close - btccandlestick[btccandlestick.Count - i].Open) / btccandlestick[btccandlestick.Count - i].Open;
            }
            sondortmumortalama /= 4;
            Debug.WriteLine($"Analiz2-SonDort#1");
            if (sondortmumortalama < -0.003)
                return true;

            //2
            sondortmumortalama = 0;
            Debug.WriteLine($"Analiz2-SonDort#2");
            for (int i = 1; i < 11; i++)
            {
                sondortmumortalama = ((double)btccandlestick[btccandlestick.Count - i].Close - btccandlestick[btccandlestick.Count - i].Open) / btccandlestick[btccandlestick.Count - i].Open;
                if (sondortmumortalama < -0.01)
                    return true;
            }
            Debug.WriteLine($"Analiz2-Son");

            return false;
        }
        public void Analiz3(bool onlycalculateLMTS)
        {

        }
    }
}
