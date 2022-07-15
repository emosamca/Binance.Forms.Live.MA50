using Binance.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Indicator
{
    public class Analizci
    {
        public float Sonmumalimbaskisi;
        public float Gunlukalimbaskisi;
        public float SonAltiSaatlikalimbaskisi;
        public float Volort;
        public float AlimVolort;
        public float VolArtis;
        public float AlimVolArtis;
        public float SatimVolArtis;
        public Analizci()
        {

        }
        public void GetVolumeAnaliz(List<Candlestick> mums)
        {
            Sonmumalimbaskisi = (mums[mums.Count - 2].TakerBuyQuoteAssetVolume / mums[mums.Count - 2].QuoteAssetVolume) * 100;
            float totalvol = 0;
            float alimvol = 0;
            for(int start=mums.Count-26;start<mums.Count-1;start++)
            {
                totalvol += mums[start].QuoteAssetVolume;
                alimvol += mums[start].TakerBuyQuoteAssetVolume;
            }
            Gunlukalimbaskisi = (alimvol / totalvol) * 100;
            totalvol = 0;
            alimvol = 0;
            for (int start = mums.Count - 14; start < mums.Count - 1; start++)
            {
                totalvol += mums[start].QuoteAssetVolume;
                alimvol += mums[start].TakerBuyQuoteAssetVolume;
            }
            SonAltiSaatlikalimbaskisi = (alimvol / totalvol) * 100;
        }
        public void GetVolumeOrtalama(List<Candlestick> mums,string interveal)
        {
            float totalvol = 0;
            float alimvol = 0;
            if (interveal == "1h")
            {
                for (int start = mums.Count - 26; start < mums.Count - 2; start++)
                {
                    totalvol += mums[start].QuoteAssetVolume;
                    alimvol += mums[start].TakerBuyQuoteAssetVolume;
                }
                Volort = totalvol / 24;
                AlimVolort = alimvol / 24;
            }
            else if (interveal == "4h")
            {
                for (int start = mums.Count - 8; start < mums.Count - 2; start++)
                {
                    totalvol += mums[start].QuoteAssetVolume;
                    alimvol += mums[start].TakerBuyQuoteAssetVolume;
                }
                Volort = totalvol / 6;
                AlimVolort = alimvol / 6;
            }
            else if (interveal == "1d")
            {
                for (int start = mums.Count - 9; start < mums.Count - 2; start++)
                {
                    totalvol += mums[start].QuoteAssetVolume;
                    alimvol += mums[start].TakerBuyQuoteAssetVolume;
                }
                Volort = totalvol / 7;
                AlimVolort = alimvol / 7;
            }
        }
        public void GetBesDakikalikAnaliz(List<Candlestick> mums)
        {
            if (mums.Count != 13)
                return;
            VolArtis = 0;
            AlimVolArtis = 0;
            for(int sayac=0;sayac<12;sayac++)
            {
                VolArtis += mums[sayac].QuoteAssetVolume;
                AlimVolArtis += mums[sayac].TakerBuyQuoteAssetVolume;
            }
            VolArtis = VolArtis / 12;
            AlimVolArtis = AlimVolArtis / 12;
            VolArtis = mums[12].QuoteAssetVolume / VolArtis;
            AlimVolArtis = mums[12].TakerBuyQuoteAssetVolume / AlimVolArtis;
        }
        public void GetOnBesDakikalikAnaliz(List<Candlestick> mums)
        {
            VolArtis = 0;
            AlimVolArtis = 0;
            SatimVolArtis = 0;
            int basl = mums.Count - 13;
            for (int sayac = basl; sayac < mums.Count - 1; sayac++)
            {
                VolArtis += mums[sayac].QuoteAssetVolume;
                AlimVolArtis += mums[sayac].TakerBuyQuoteAssetVolume;
                SatimVolArtis += (mums[sayac].QuoteAssetVolume - mums[sayac].TakerBuyQuoteAssetVolume);
            }
            VolArtis = VolArtis / 12;
            AlimVolArtis = AlimVolArtis / 12;
            SatimVolArtis = SatimVolArtis / 12;
            VolArtis = (mums[mums.Count - 1].QuoteAssetVolume / VolArtis) * 100;
            AlimVolArtis = (mums[mums.Count - 1].TakerBuyQuoteAssetVolume / AlimVolArtis) * 100;
            SatimVolArtis = ((mums[mums.Count - 1].QuoteAssetVolume - mums[mums.Count - 1].TakerBuyQuoteAssetVolume) / SatimVolArtis) * 100;
        }
    }
}
