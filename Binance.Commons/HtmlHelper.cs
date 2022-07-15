using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public static class HtmlHelper
    {
        public static string PublicIp { get; set; }
        static object lockobj = new object();
        private static string dizin = @"C:\wamp64\www";
        public static string GetHtmlTableCode(List<AnalizTrack> liste)
        {
            lock (lockobj)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<!doctype html>");
                builder.AppendLine("<html>");
                builder.AppendLine("<head>");
                builder.AppendLine("<title> Alımı Devam Edenler </title>");
                builder.AppendLine("<meta charset = \"utf -8\"/>");
                builder.AppendLine("<meta http-equiv=\"refresh\" content=\"120\" />");
                builder.AppendLine("</head>");
                builder.AppendLine("<body>");
                var listeorderdate = liste.OrderByDescending(x => x.Date);
                builder.AppendLine(GetTableRows(listeorderdate.ToList(), "Tarihe Göre Sıralama", 1).ToString());
                builder.AppendLine("<tr/>");
                var listeorder = liste.OrderByDescending(x => x.CurrentProfit);
                builder.AppendLine(GetTableRows(listeorder.ToList(), "Şu Andaki Kazanca Göre Sıralama", 1).ToString());
                builder.AppendLine("<tr/>");
                var listecurorder = liste.OrderByDescending(x => x.MaxProfit);
                builder.AppendLine(GetTableRows(listecurorder.ToList(), "Maksimum Kazanca Göre Sıralama", 2).ToString());
                builder.AppendLine("<tr/>");
                var listecoinnameorder = liste.OrderBy(x => x.Name);
                builder.AppendLine(GetTableRows(listecoinnameorder.ToList(), "İsme Göre Sıralama", 3).ToString());
                builder.AppendLine("</body>");
                builder.AppendLine("</html>");
                File.WriteAllText(@"{dizin}\emo.htm", builder.ToString(), Encoding.UTF8);
            }
            return "";
        }
        private static StringBuilder GetTableRows(List<AnalizTrack> liste, string header, int tip)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine($"<tr><th colspan=\"13\">{header}</th></tr>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Tarih</th>");
            builder.AppendLine("<th>Süre</th>");
            builder.AppendLine("<th>Al.Tipi</th>");
            builder.AppendLine("<th>Koin</th>");
            builder.AppendLine("<th>Alış Fiyatı</th>");
            //builder.AppendLine("<th>Durumu</th>");
            //builder.AppendLine("<th>Satış Fiyatı</th>");
            //builder.AppendLine("<th>Kazanç</th>");
            builder.AppendLine("<th>Maks.Fiyat</th>");
            builder.AppendLine("<th>MaxKaz</th>");
            builder.AppendLine("<th>Anlık Fiyatı</th>");
            builder.AppendLine("<th>An.Kaz.</th>");
            builder.AppendLine("<th>Vol%</th>");
            builder.AppendLine("<th>AlVol%</th>");
            builder.AppendLine("<th>Süre</th>");
            builder.AppendLine("<th>Seviyeler</th>");
            builder.AppendLine("</tr>");
            foreach (var item in liste)
            {
                string renk = "";
                string renk2 = "";
                if (tip == 1 || tip == 3)
                    if (item.CurrentProfit <= 0)
                        builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                    else
                        builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");
                if (tip == 2)
                    if (item.MaxProfit < 0)
                        builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                    else
                        builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");
                if (item.CurrentProfit > 0)
                    renk = "\"#00FF00\"";
                else
                    renk = "\"#FFCBDB\"";
                if (item.MaxProfit > 0)
                    renk2 = "\"#00FF00\"";
                else
                    renk2 = "\"#FFCBDB\"";

                builder.AppendLine($"<td>{item.Datestr}</td>");   //1
                builder.AppendLine($"<td>{item.Sure}</td>");    //2
                string alimtype = item.AlimType == 0 ? "Normal" : item.AlimType == 1 ? "5 Dk." : item.AlimType == 2? "TEST":"OKAN";
                builder.AppendLine($"<td>{alimtype}</td>");    //2
                builder.AppendLine($"<td>{item.Name}</td>");    //3
                builder.AppendLine($"<td>{item.Price.ToString("F8")}</td>");    //4
                //builder.AppendLine($"<td>{item.State}</td>");
                //builder.AppendLine($"<td>{item.SellPrice.ToString("F8")}</td>");
                //builder.AppendLine($"<td>{item.Profit.ToString("F1")}</td>");
                builder.AppendLine($"<td>{item.MaxPrice.ToString("F8")}</td>"); //5
                builder.AppendLine($"<td BGCOLOR={renk2}>{item.MaxProfit.ToString("F2")}</td>");    //6
                builder.AppendLine($"<td>{item.CurrentPrice.ToString("F8")}</td>"); //7
                builder.AppendLine($"<td BGCOLOR={renk}>{item.CurrentProfit.ToString("F2")}</td>");    //8
                builder.AppendLine($"<td>{item.Volort.ToString("F2")}</td>");   //9
                builder.AppendLine($"<td>{item.AlimVolort.ToString("F2")}</td>");   //10
                builder.AppendLine($"<td>{DateHelper.GetDateDiff(item.Date, DateHelper.GetCurrentTimeStam()).ToString("F1")}</td>");    //11
                builder.AppendLine($"<td>{item.Levels}</td>");   //10
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            return builder;
        }
        public static string GetMAInfoTableCode(List<MAcoin> liste, string Filename)
        {
            lock (lockobj)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<!doctype html>");
                builder.AppendLine("<html>");
                builder.AppendLine("<head>");
                builder.AppendLine("<title> MA bilgi </title>");
                builder.AppendLine("<meta charset = \"utf -8\"/>");
                builder.AppendLine("<meta http-equiv=\"refresh\" content=\"120\" />");
                builder.AppendLine("</head>");
                builder.AppendLine("<body>");
                var ma50yeniust = liste.FindAll(x => x.MAtypes==MAType.MA50yeniust).OrderBy(x=>x.Name).ToList();
                if (ma50yeniust != null)
                {
                    builder.AppendLine(GetMAInfoTableRows(ma50yeniust, "MA50 yeni üst olanlar").ToString());
                    builder.AppendLine("<tr/>");
                }
                var ma50yenialt = liste.FindAll(x => x.MAtypes == MAType.MA50yenialt).OrderBy(x => x.Name).ToList();
                if (ma50yenialt != null)
                {
                    builder.AppendLine(GetMAInfoTableRows(ma50yenialt, "MA50 yeni alt olanlar").ToString());
                    builder.AppendLine("<tr/>");
                }
                var ma50ust = liste.FindAll(x => x.MAtypes == MAType.MA50ustu).OrderBy(x => x.Name).ToList();
                if (ma50ust != null)
                {
                    builder.AppendLine(GetMAInfoTableRows(ma50ust, "MA50 üstü olanlar").ToString());
                    builder.AppendLine("<tr/>");
                }
                var ma50alt = liste.FindAll(x => x.MAtypes == MAType.MA50alt).OrderBy(x => x.Name).ToList();
                if (ma50alt != null)
                {
                    builder.AppendLine(GetMAInfoTableRows(ma50alt, "MA50 altı olanlar").ToString());
                    builder.AppendLine("<tr/>");
                }

                builder.AppendLine("</body>");
                builder.AppendLine("</html>");
                File.WriteAllText($"{dizin}\\{Filename}.htm", builder.ToString(), Encoding.UTF8);
            }
            return "";
        }

        public static void GetCoinDurum(List<Candlestick> sonuc)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<!doctype html>");
            builder.AppendLine("<html>");
            builder.AppendLine("<head>");
            builder.AppendLine("<title> Coin MTS Sonuc </title>");
            builder.AppendLine("<meta charset = \"utf -8\"/>");
            //builder.AppendLine("<meta http-equiv=\"refresh\" content=\"120\" />");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine($"<tr><th colspan=\"7\">{sonuc[0].Name} Bilgi</th></tr>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Coin</th>");
            builder.AppendLine("<th>Mum açılış</th>");
            builder.AppendLine("<th>Open</th>");
            builder.AppendLine("<th>Close</th>");
            builder.AppendLine("<th>High</th>");
            builder.AppendLine("<th>Low</th>");
            builder.AppendLine("<th>MTS</th>");
            builder.AppendLine("</tr>");
            foreach (var item in sonuc)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine($"<td>{item.Name}</td>");   //1
                builder.AppendLine($"<td>{item.OpenTimeStr}</td>");    //2
                builder.AppendLine($"<td>{item.Open.ToString().Replace(".", ",")}</td>");    //2
                builder.AppendLine($"<td>{((float)item.Close).ToString().Replace(".", ",")}</td>");    //2
                builder.AppendLine($"<td>{item.High.ToString().Replace(".", ",")}</td>");    //2
                builder.AppendLine($"<td>{item.Low.ToString().Replace(".", ",")}</td>");    //2
                builder.AppendLine($"<td>{item.MTS.ToString().Replace(".", ",")}</td>");    //2
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");
            File.WriteAllText($"{dizin}\\{sonuc[0].Name}.htm", builder.ToString(), Encoding.UTF8);
        }

        private static StringBuilder GetMAInfoTableRows(List<MAcoin> liste, string header)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine($"<tr><th colspan=\"6\">{header}</th></tr>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Coin</th>");
            builder.AppendLine("<th>Önceki mum MA50 durumu</th>");
            builder.AppendLine("<th>MA50 üstü</th>");
            builder.AppendLine("<th>MA100 üstü</th>");
            builder.AppendLine("<th>MA200 üstü</th>");
            builder.AppendLine("<th>MA sıra</th>");
            builder.AppendLine("</tr>");
            foreach (var item in liste)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine($"<td>{item.Name}</td>");   //1
                if (item.MAtypes == MAType.MA50alt)
                    builder.AppendLine($"<td>Alt</td>");    //2
                else if (item.MAtypes == MAType.MA50ustu)
                    builder.AppendLine($"<td>Üst</td>");    //2
                else if (item.MAtypes == MAType.MA50yenialt)
                    builder.AppendLine($"<td>Yeni Alt</td>");    //2
                else
                    builder.AppendLine($"<td>Yeni Üst</td>");    //2

                if (item.MA50ustu)
                    builder.AppendLine($"<td>Evet</td>");    //2
                else
                    builder.AppendLine($"<td>Hayır</td>");    //2
                if (item.MA50ilk)
                {
                    builder.AppendLine($"<td>---</td>");    //2
                    builder.AppendLine($"<td>---</td>");    //2
                }
                else
                {
                    if (item.MA100ustu)
                        builder.AppendLine($"<td>Evet</td>");    //2
                    else
                        builder.AppendLine($"<td>Hayır</td>");    //2
                    if (item.MA200ustu)
                        builder.AppendLine($"<td>Evet</td>");    //2
                    else
                        builder.AppendLine($"<td>Hayır</td>");    //2
                }
                builder.AppendLine($"<td>{item.MAorder}</td>");    //2
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            return builder;
        }
        public static string GetIzlemeHtmlCode(List<Izleme> izlemelist)
        {
            lock (lockobj)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<!doctype html>");
                builder.AppendLine("<html>");
                builder.AppendLine("<head>");
                builder.AppendLine("<title> İzleme Listesi </title>");
                builder.AppendLine("<meta charset = \"utf -8\"/>");
                builder.AppendLine("<meta http-equiv=\"refresh\" content=\"60\" />");
                builder.AppendLine("</head>");
                builder.AppendLine("<body>");
                builder.AppendLine($"<b>Güncelleme zamanı : {DateTime.Now.ToString()}</b>");
                builder.AppendLine("<br>");

                builder.AppendLine("<table border = 1 >");
                builder.AppendLine($"<tr><th colspan=\"7\">İzleme Listesi</th></tr>");
                builder.AppendLine("<tr>");
                builder.AppendLine("<th>S/N</th>");
                builder.AppendLine("<th>Sembol</th>");
                builder.AppendLine("<th>Süre</th>");
                builder.AppendLine("<th>Volort</th>");
                builder.AppendLine("<th>AlimVolOrt</th>");
                builder.AppendLine("<th>Tarih</th>");
                builder.AppendLine("<th>Bilgi</th>");
                builder.AppendLine("</tr>");
                int sn = 1;
                foreach (var item in izlemelist)
                {
                    builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                    builder.AppendLine($"<td>{sn}</td>");   //1
                    builder.AppendLine($"<td>{item.Symbol}</td>");    //2
                    builder.AppendLine($"<td>{item.Interval}</td>");    //3
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.Volort.ToString("F8")}</td>");    //4
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.Alimort.ToString("F8")}</td>");    //4
                    builder.AppendLine($"<td>{item.Datestr}</td>"); //5
                    builder.AppendLine($"<td>{item.Mesaj}</td>");   //10
                    builder.AppendLine("</tr>");
                    sn++;
                }
                builder.AppendLine("</table>");
                builder.AppendLine("<br>");
                builder.AppendLine("</body>");
                builder.AppendLine("</html>");
                File.WriteAllText(@"{dizin}\besdakikaizleme.htm", builder.ToString(), Encoding.UTF8);
            }
            return "";

        }
        public static string GetHtmlTableCodeAll(List<AnalizTrack> liste)
        {
            lock (lockobj)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<!doctype html>");
                builder.AppendLine("<html>");
                builder.AppendLine("<head>");
                builder.AppendLine("<title> Tablo Uygulaması </title>");
                builder.AppendLine("<meta charset = \"utf -8\"/>");
                builder.AppendLine("<meta http-equiv=\"refresh\" content=\"240\" />");
                builder.AppendLine("</head>");
                builder.AppendLine("<body>");
                var listeorder = liste.OrderByDescending(x => x.Selldate);
                builder.AppendLine(GetTableRowsAll(listeorder.ToList(), "Satış Tarihine Göre Sıralama", 1).ToString());
                builder.AppendLine("</body>");
                builder.AppendLine("</html>");
                File.WriteAllText(@"{dizin}\hepsi.htm", builder.ToString(), Encoding.UTF8);
            }
            return "";
        }
        private static StringBuilder GetTableRowsAll(List<AnalizTrack> liste, string header, int tip)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine($"<tr><th colspan=\"13\">{header}</th></tr>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Tarih</th>"); //0
            builder.AppendLine("<th>Süre</th>");  //1
            builder.AppendLine("<th>Al.Tipi</th>");  //2
            builder.AppendLine("<th>Durum</th>");  //2
            builder.AppendLine("<th>Koin</th>");  //3
            builder.AppendLine("<th>Al.Fiyatı</th>");   //4
            builder.AppendLine("<th>Maks.Fiyat</th>");
            builder.AppendLine("<th>Maks.Kazanç</th>");
            builder.AppendLine("<th>An.Fiyat</th>");
            builder.AppendLine("<th>An.Kazanç</th>");
            builder.AppendLine("<th>Süre</th>");
            builder.AppendLine("<th>Sat.Tarihi</th>");
            builder.AppendLine("<th>Seviyeler</th>");
            builder.AppendLine("</tr>");
            foreach (var item in liste)
            {
                string renk = "";
                string renk2 = "";
                if (tip == 1 || tip == 3)
                    if (item.CurrentProfit <= 0)
                        builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                    else
                        builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");
                if (tip == 2)
                    if (item.MaxProfit < 0)
                        builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                    else
                        builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");
                if (item.CurrentProfit > 0)
                    renk = "\"#00FF00\"";
                else
                    renk = "\"#FFCBDB\"";
                if (item.MaxProfit > 0)
                    renk2 = "\"#00FF00\"";
                else
                    renk2 = "\"#FFCBDB\"";
                builder.AppendLine($"<td>{item.Datestr}</td>");   //1
                builder.AppendLine($"<td>{item.Sure}</td>");    //2
                string alimtype = item.AlimType == 0 ? "Normal" : item.AlimType == 1 ? "5 Dk." : item.AlimType == 2 ? "TEST" : "OKAN";
                builder.AppendLine($"<td>{alimtype}</td>");    //2
                builder.AppendLine($"<td>{item.State}</td>");    //2
                builder.AppendLine($"<td>{item.Name}</td>");    //3
                builder.AppendLine($"<td>{item.Price.ToString("F8")}</td>");    //4
                builder.AppendLine($"<td>{item.MaxPrice.ToString("F8")}</td>"); //5
                builder.AppendLine($"<td BGCOLOR={renk2}>{item.MaxProfit.ToString("F2")}</td>");    //6
                builder.AppendLine($"<td>{item.CurrentPrice.ToString("F8")}</td>"); //7
                builder.AppendLine($"<td BGCOLOR={renk}>{item.CurrentProfit.ToString("F2")}</td>");    //8
                builder.AppendLine($"<td>{DateHelper.GetDateDiff(item.Date, item.Selldate).ToString("F1")}</td>");    //11
                builder.AppendLine($"<td>{item.Selldatestr}</td>");   //10
                builder.AppendLine($"<td>{item.Levels}</td>");   //10
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            return builder;
        }



        public static void GetOrderHtmlTable(List<Order> htmlorders, UsersBudget usersBudget1, double GercekUSDT, List<Order> tumemirler)
        {
            lock (lockobj)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<!doctype html>");
                builder.AppendLine("<html>");
                builder.AppendLine("<head>");
                builder.AppendLine($"<title> {usersBudget1.HtmlHeader} </title>");
                builder.AppendLine("<meta charset = \"utf -8\"/>");
                builder.AppendLine("<meta http-equiv=\"refresh\" content=\"15\" />");
                builder.AppendLine("</head>");
                builder.AppendLine("<body>");
                builder.AppendLine("<br>");
                builder.AppendLine($"<b>Strateji : {usersBudget1.HtmlHeader}-{usersBudget1.Seviyeler}</b>");
                builder.AppendLine("<br>");
                builder.AppendLine("<table border = 1 >");
                builder.AppendLine($"<tr><th colspan=\"6\">Cüzdan Bilgileri</th></tr>");
                builder.AppendLine("<tr>");
                builder.AppendLine("<th>Başlangıç</th>");
                builder.AppendLine("<th>Komisyon</th>");
                builder.AppendLine("<th>Kalan Para</th>");
                builder.AppendLine("<th>Şu anki değerlerle toplam para</th>");
                builder.AppendLine("<th>Trade Sayısı</th>");
                builder.AppendLine("<th>Max.Trade</th>");
                builder.AppendLine("</tr>");
                builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");
                builder.AppendLine($"<td>{usersBudget1.StartBudget.ToString("F8")}</td>");   //10
                builder.AppendLine($"<td>{usersBudget1.TargetBudget.ToString("F8")}</td>");   //10
                builder.AppendLine($"<td>{usersBudget1.RemainingMoney.ToString("F8")}</td>");   //10
                builder.AppendLine($"<td>{GercekUSDT.ToString("F8")}</td>");   //10
                builder.AppendLine($"<td>{usersBudget1.TradeNow.ToString("F0")}</td>");   //10
                builder.AppendLine($"<td>{usersBudget1.TradeMax.ToString("F0")}</td>");   //10
                builder.AppendLine("</tr>");
                builder.AppendLine("</table>");
                builder.AppendLine("<br>");
                builder.AppendLine($"<b>Güncelleme zamanı : {DateTime.Now.ToString()}</b>");
                builder.AppendLine("<br>");
                builder.AppendLine("<table border = 1 >");
                builder.AppendLine($"<tr><th colspan=\"11\">Alım Satım Bilgileri</th></tr>");
                builder.AppendLine("<tr>");
                builder.AppendLine("<th>S/N</th>");
                builder.AppendLine("<th>Sembol</th>");
                builder.AppendLine("<th>Fiyat</th>");
                builder.AppendLine("<th>Adet</th>");
                builder.AppendLine("<th>Durum</th>");
                builder.AppendLine("<th>Tarih</th>");
                builder.AppendLine("<th>Anlık Fiyat</th>");
                builder.AppendLine("<th>Kazanç</th>");
                builder.AppendLine("<th>Toplam</th>");
                builder.AppendLine("<th>Interval</th>");
                builder.AppendLine("<th>Seviye</th>");
                builder.AppendLine("</tr>");
                int sn = 1;
                foreach (var item in htmlorders)
                {
                    if (item.CurrentProfit <= 0)
                        builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                    else
                        builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");

                    builder.AppendLine($"<td>{sn}</td>");   //1
                    builder.AppendLine($"<td>{item.symbol}</td>");    //2
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.price.ToString("F8")}</td>");    //3
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.origQty.ToString("F8")}</td>");    //4
                    builder.AppendLine($"<td>{item.status}</td>"); //5
                    builder.AppendLine($"<td>{DateHelper.GetDateStrFromTimeStamp(item.time)}</td>");    //11
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.CurrentPrice.ToString("F8")}</td>"); //7
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.CurrentProfit.ToString("F2")}</td>");   //9
                    if(usersBudget1.UserName.EndsWith("USDT"))
                        builder.AppendLine($"<td>{item.TotalPrice.ToString("F2")}</td>");   //10
                    else if (usersBudget1.UserName.EndsWith("BTC"))
                        builder.AppendLine($"<td>{item.TotalPrice.ToString("F8")}</td>");   //10
                    builder.AppendLine($"<td>{item.Sure}</td>");   //10
                    builder.AppendLine($"<td>{item.Seviye}</td>");   //10
                    builder.AppendLine("</tr>");
                    sn++;
                }
                builder.AppendLine("</table>");
                builder.AppendLine("<br>");


                builder.AppendLine("<table border = 1 >");
                builder.AppendLine($"<tr><th colspan=\"11\">Tüm Alım Satım Bilgileri</th></tr>");
                builder.AppendLine("<tr>");
                builder.AppendLine("<th>S/N</th>");
                builder.AppendLine("<th>Sembol</th>");
                builder.AppendLine("<th>Fiyat</th>");
                builder.AppendLine("<th>Adet</th>");
                builder.AppendLine("<th>Durum</th>");
                builder.AppendLine("<th>Tarih</th>");
                builder.AppendLine("<th>Anlık Fiyat</th>");
                builder.AppendLine("<th>Kazanç</th>");
                builder.AppendLine("<th>Toplam</th>");
                builder.AppendLine("<th>Interval</th>");
                builder.AppendLine("<th>Seviye</th>");
                builder.AppendLine("</tr>");
                sn = 1;
                foreach (var item in tumemirler)
                {
                    if (item.CurrentProfit <= 0)
                        builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                    else
                        builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");

                    builder.AppendLine($"<td>{sn}</td>");   //1
                    builder.AppendLine($"<td>{item.symbol}</td>");    //2
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.price.ToString("F8")}</td>");    //3
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.origQty.ToString("F8")}</td>");    //4
                    builder.AppendLine($"<td>{item.side + "_" + item.status}</td>"); //5
                    builder.AppendLine($"<td>{DateHelper.GetDateStrFromTimeStamp(item.time)}</td>");    //11
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.CurrentPrice.ToString("F8")}</td>"); //7
                    builder.AppendLine($"<td style=\"text-align:right;\">{item.CurrentProfit.ToString("F2")}</td>");   //9
                    builder.AppendLine($"<td>{item.TotalPrice.ToString("F2")}</td>");   //10
                    builder.AppendLine($"<td>{item.Sure}</td>");   //10
                    builder.AppendLine($"<td>{item.Seviye}</td>");   //10
                    builder.AppendLine("</tr>");
                    sn++;
                }
                builder.AppendLine("</table>");
                File.WriteAllText($@"{dizin}\trades{usersBudget1.UserName}.htm", builder.ToString(), Encoding.UTF8);

            }
        }

        public static void GetBuyTableHtmlTable(List<BuyTable> aktifler, List<BuyTable> pasifler,string pagename)
        {
            lock (lockobj)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<!doctype html>");
                builder.AppendLine("<html>");
                builder.AppendLine("<head>");
                builder.AppendLine("<title> Öğüşlü Metod Alımı Devam Edenler </title>");
                builder.AppendLine("<meta charset = \"utf -8\"/>");
                builder.AppendLine("<meta http-equiv=\"refresh\" content=\"60\" />");
                builder.AppendLine("</head>");
                builder.AppendLine("<body>");
                builder.AppendLine(GetOgusluRows(aktifler, "Durumu devam edenler").ToString());
                builder.AppendLine("<tr/>");
                builder.AppendLine(GetOgusluRows(pasifler, "Durumu satıldı olanlar").ToString());
                builder.AppendLine("<tr/>");
                builder.AppendLine("</body>");
                builder.AppendLine("</html>");
                File.WriteAllText($@"{dizin}\{pagename}.htm", builder.ToString(), Encoding.UTF8);
            }
        }

        private static StringBuilder GetOgusluRows(List<BuyTable> liste, string header)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine($"<tr><th colspan=\"15\">{header}</th></tr>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Koin</th>");
            builder.AppendLine("<th>İkaz No</th>");
            builder.AppendLine("<th>Al.zamanı</th>");
            builder.AppendLine("<th>Al.fiyatı</th>");
            builder.AppendLine("<th>Anl.fiyat</th>");
            builder.AppendLine("<th>Anl.kazanç</th>");
            builder.AppendLine("<th>MaxKaz</th>");
            builder.AppendLine("<th>MinKaz</th>");
            builder.AppendLine("<th>Satış Kaz.</th>");
            builder.AppendLine("<th>Satış Tar.</th>");
            builder.AppendLine("<th>ATR satış</th>");
            builder.AppendLine("<th>ATR SL</th>");
            builder.AppendLine("<th>ATR VAL</th>");
            builder.AppendLine("<th>Açıklama</th>");
            builder.AppendLine("<th>ATR durum</th>");
            builder.AppendLine("</tr>");
            foreach (var item in liste)
            {
                if (item.Profit <= 0)
                    builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                else
                    builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");

                builder.AppendLine($"<td>{item.CoinName}</td>");   //1
                builder.AppendLine($"<td>{item.IkazNo}</td>");    //2
                builder.AppendLine($"<td>{item.BuyDatestr}</td>");    //2
                builder.AppendLine($"<td>{item.BuyPrice.ToString("F8")}</td>");    //3
                builder.AppendLine($"<td>{item.Price.ToString("F8")}</td>");    //4
                builder.AppendLine($"<td>{item.Profit.ToString("F2")}</td>"); //5
                builder.AppendLine($"<td>{item.MaxProfit.ToString("F2")}</td>"); //7
                builder.AppendLine($"<td>{item.MinProfit.ToString("F2")}</td>"); //7
                builder.AppendLine($"<td>{item.DayProfit}</td>"); //7
                builder.AppendLine($"<td>{item.SellDatestr}</td>"); //7
                builder.AppendLine($"<td>{item.atrsatis.ToString("F8")}</td>"); //7
                builder.AppendLine($"<td>{item.atrstoploss.ToString("F8")}</td>"); //7
                builder.AppendLine($"<td>{item.atrval.ToString("F8")}</td>"); //7
                builder.AppendLine($"<td>{item.atrresult}</td>"); //7
                builder.AppendLine($"<td>{item.atrprofit.ToString("F2")}</td>"); //7
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            return builder;
        }

        public static void GetDipBuyTableHtmlTable(List<NewBuyTable> aktifler, List<NewBuyTable> pasifler, string pagename, UsersBudget usersBudget1, double gercekusdt, List<NewBuyTable> alinacaklar)
        {
            lock (lockobj)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<!doctype html>");
                builder.AppendLine("<html>");
                builder.AppendLine("<head>");
                builder.AppendLine($"<title> Öğüşlü({usersBudget1.UserName}-{usersBudget1.HtmlHeader}) </title>");
                builder.AppendLine("<meta charset = \"utf -8\"/>");
                builder.AppendLine("<meta http-equiv=\"refresh\" content=\"60\" />");
                builder.AppendLine("</head>");
                builder.AppendLine("<body>");
                if (GlobalVars.btc50up)
                    builder.AppendLine("<b><p style=\"color:green\">Piyasa YEŞİL</p></b>");
                else
                    builder.AppendLine("<b><p style=\"color:red\">Piyasa KIRMIZI</p></b>");
                if (GlobalVars.btc20up)
                    builder.AppendLine("<b><p style=\"color:green\">BTC MA20 üstü</p></b>");
                else
                    builder.AppendLine("<b><p style=\"color:red\">BTC MA20 altı</p></b>");
                builder.AppendLine($"<h1>Cüzdan ID: {usersBudget1.Id}</h1>");
                builder.AppendLine("<table border = 1 >");
                builder.AppendLine($"<tr><th colspan=\"9\">Cüzdan Bilgileri</th></tr>");
                builder.AppendLine("<tr>");
                builder.AppendLine("<th>Başlangıç</th>");
                builder.AppendLine("<th>Komisyon</th>");
                builder.AppendLine("<th>Kalan Para</th>");
                builder.AppendLine("<th>Toplam para</th>");
                builder.AppendLine("<th>Top.para(Kom.çıkınca)</th>");
                builder.AppendLine("<th>Kazanç</th>");
                builder.AppendLine("<th>Kazanç(Kom.Har.)</th>");
                builder.AppendLine("<th>Trade Sayısı</th>");
                builder.AppendLine("<th>Max.Trade</th>");
                builder.AppendLine("</tr>");
                builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");
                builder.AppendLine($"<td>{usersBudget1.StartBudget.ToString("F3")}</td>");   //10
                builder.AppendLine($"<td>{usersBudget1.TargetBudget.ToString("F8")}</td>");   //10
                builder.AppendLine($"<td>{usersBudget1.RemainingMoney.ToString("F3")}</td>");   //10
                builder.AppendLine($"<td>{gercekusdt.ToString("F3")}</td>");   //10
                builder.AppendLine($"<td>{(gercekusdt - usersBudget1.TargetBudget).ToString("F3")}</td>");   //10
                if (pagename == "emrahMAbutce")
                {
                    builder.AppendLine($"<td>%{(((gercekusdt / 2000) - 1) * 100).ToString("F2")}</td>");   //10
                    builder.AppendLine($"<td>%{((((gercekusdt - usersBudget1.TargetBudget) / 2000) - 1) * 100).ToString("F2")}</td>");   //10
                }
                else
                {
                    builder.AppendLine($"<td>%{(((gercekusdt / usersBudget1.StartBudget) - 1) * 100).ToString("F2")}</td>");   //10
                    builder.AppendLine($"<td>%{((((gercekusdt - usersBudget1.TargetBudget) / usersBudget1.StartBudget) - 1) * 100).ToString("F2")}</td>");   //10
                }
                builder.AppendLine($"<td>{usersBudget1.TradeNow.ToString("F0")}</td>");   //10
                builder.AppendLine($"<td>{usersBudget1.TradeMax.ToString("F0")}</td>");   //10
                builder.AppendLine("</tr>");
                builder.AppendLine("</table>");
                builder.AppendLine(GetOgusluDipRows(alinacaklar, "Alınmayı bekleyenler").ToString());
                builder.AppendLine("<tr/>");
                builder.AppendLine(GetOgusluDipRows(aktifler, "Durumu devam edenler").ToString());
                builder.AppendLine("<tr/>");
                builder.AppendLine(GetOgusluDipRows(pasifler, "Durumu satıldı olanlar").ToString());
                builder.AppendLine("<tr/>");
                builder.AppendLine("</body>");
                builder.AppendLine("</html>");
                File.WriteAllText($@"{dizin}\{pagename}.htm", builder.ToString(), Encoding.UTF8);
            }
        }
        private static StringBuilder GetOgusluDipRows(List<NewBuyTable> liste, string header)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine($"<tr><th colspan=\"19\">{header}</th></tr>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Koin</th>");
            builder.AppendLine("<th>Dahil</th>");
            builder.AppendLine("<th>Al.zamanı</th>");
            builder.AppendLine("<th>Al.fiyatı</th>");
            builder.AppendLine("<th>Alış MTS</th>");
            builder.AppendLine("<th>Anl.fiyat</th>");
            builder.AppendLine("<th>Anl.kazanç</th>");
            builder.AppendLine("<th>Sat.fiyat</th>");
            builder.AppendLine("<th>Satış Kaz.</th>");
            builder.AppendLine("<th>Satış Tar.</th>");
            builder.AppendLine("<th>Satış MTS</th>");
            builder.AppendLine("<th>Satış Seb.</th>");
            builder.AppendLine("<th>MaxKaz</th>");
            builder.AppendLine("<th>MinKaz</th>");
            builder.AppendLine("<th>Alım($)</th>");
            builder.AppendLine("<th>Alım adet</th>");
            builder.AppendLine("<th>Satış($)</th>");
            builder.AppendLine("<th>Satılacak MTS</th>");
            builder.AppendLine("<th>Track Stop</th>");

            builder.AppendLine("</tr>");
            foreach (var item in liste)
            {
                if (item.ProfitNow <= 0)
                    builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                else
                    builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");

                builder.AppendLine($"<td>{item.CoinName}</td>");   //1
                if(item.TotalMoney==0)
                    builder.AppendLine($"<td></td>");   //1
                else
                    builder.AppendLine($"<td>X</td>");   //1
                builder.AppendLine($"<td>{item.BuyDatestr}</td>");    //2
                builder.AppendLine($"<td>{item.BuyPrice.ToString("F8")}</td>");    //2
                builder.AppendLine($"<td>{item.AlisMTS.ToString("F3")}</td>");    //3
                builder.AppendLine($"<td>{item.PriceNow.ToString("F8")}</td>");    //4
                builder.AppendLine($"<td>{item.ProfitNow.ToString("F2")}</td>"); //5
                builder.AppendLine($"<td>{item.SellPrice.ToString("F8")}</td>"); //7
                builder.AppendLine($"<td>{item.SellProfit.ToString("F2")}</td>"); //8
                builder.AppendLine($"<td>{item.SellDatestr}</td>"); //9
                builder.AppendLine($"<td>{item.SellCandleMTS.ToString("F3")}</td>"); //10
                builder.AppendLine($"<td>{item.SellReason}</td>"); //11
                builder.AppendLine($"<td>{item.MaxProfit.ToString("F2")}</td>"); //12
                builder.AppendLine($"<td>{item.MinProfit.ToString("F2")}</td>"); //13
                builder.AppendLine($"<td>{item.TotalMoney.ToString("F3")}</td>"); //13
                builder.AppendLine($"<td>{item.TotalAdet.ToString("F3")}</td>"); //13
                if (item.SellPrice == 0 || item.SellPrice==-1)
                    builder.AppendLine($"<td>{(item.TotalAdet * item.PriceNow).ToString("F3")}</td>"); //13
                else
                    builder.AppendLine($"<td>{(item.TotalAdet * item.SellPrice).ToString("F3")}</td>"); //13
                builder.AppendLine($"<td>{(item.AlisMTS + item.SellLMTS()).ToString("F3")}</td>"); //13
                builder.AppendLine($"<td>{(item.trackstop).ToString("F1")}</td>"); //13
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            return builder;
        }

        public static void GetMTSInfo(List<NewBuyTable> orderlist, MTS mts, List<NewBuyTable> defcoins, double ortalama, double ortalamaold, string dosyaismi, string LMTSorMTS, bool alalimmi,double cctsma20, double maxcmts=0, double mincmts=0, double stdsapma=0)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<!doctype html>");
            builder.AppendLine("<html>");
            builder.AppendLine("<head>");
            builder.AppendLine($"<title> {LMTSorMTS} Info </title>");
            builder.AppendLine("<meta charset = \"utf -8\"/>");
            builder.AppendLine("<meta http-equiv=\"refresh\" content=\"60\" />");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            if (GlobalVars.btc50up)
                builder.AppendLine("<b><p style=\"color:green\">Piyasa YEŞİL</p></b>");
            else
                builder.AppendLine("<b><p style=\"color:red\">Piyasa KIRMIZI</p></b>");
            if (GlobalVars.btc20up)
                builder.AppendLine("<b><p style=\"color:green\">BTC MA20 üstü</p></b>");
            else
                builder.AppendLine("<b><p style=\"color:red\">BTC MA20 altı</p></b>");
            builder.AppendLine("<br>");
            builder.AppendLine($"<b>Güncelleme zamanı : {DateTime.Now.ToString()}</b>");
            builder.AppendLine($"<br><b>Analize dahil olanların ortalaması : {ortalama.ToString("F4")}</b>");
            builder.AppendLine($"<br><b>Analize dahil olanların ortalaması ({LMTSorMTS} önceki) : {ortalamaold.ToString("F4")}</b>");
            builder.AppendLine($"<br><b>{LMTSorMTS} alım eşiği : {mts.MTSAl}</b>");
            builder.AppendLine($"<br><b>{LMTSorMTS} max değer  : {maxcmts}</b>");
            builder.AppendLine($"<br><b>{LMTSorMTS} min değer  : {mincmts}</b>");
            builder.AppendLine($"<br><b>{LMTSorMTS} std sapma  : {stdsapma}</b>");
            if(alalimmi)
                builder.AppendLine($"<br><b>Alım serbest</b>");
            else
                builder.AppendLine($"<br><b>Alım serbest değil.</b>");
            if(ortalama>cctsma20)
                builder.AppendLine($"<br><b>CCTS ortalama 20MA ({cctsma20}) nın üstünde</b>");
            else
                builder.AppendLine($"<br><b>CCTS ortalama 20MA ({cctsma20}) nın altında</b>");
            builder.AppendLine("<br>");
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine($"<tr><th colspan=\"5\">{LMTSorMTS} Info</th></tr>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Koin</th>");
            builder.AppendLine($"<th>{LMTSorMTS} onceki(sabit)</th>");
            builder.AppendLine($"<th>{LMTSorMTS}</th>");
            builder.AppendLine($"<th>{LMTSorMTS} tahmini</th>");
            builder.AppendLine("<th>Analize dahil?</th>");
            builder.AppendLine("</tr>");
            foreach (var item in orderlist)
            {
                if (item.AlisMTS <= mts.MTSAl)
                    builder.AppendLine("<tr BGCOLOR = \"#FFCBDB\">");
                else if (item.AlisMTS > mts.MTSSat)
                    builder.AppendLine("<tr BGCOLOR = \"#00FF00\">");
                else
                    builder.AppendLine("<tr>");
                builder.AppendLine($"<td>{item.CoinName}</td>");   //1
                builder.AppendLine($"<td>{item.MTSOld.ToString("F3")}</td>");    //2
                builder.AppendLine($"<td>{item.AlisMTS.ToString("F6")}</td>");    //2
                builder.AppendLine($"<td>{item.MTSGuess.ToString("F3")}</td>");    //2
                if (defcoins.Find(x => x.CoinName == item.CoinName) != null)
                    builder.AppendLine("<td>VAR</td>");
                else
                    builder.AppendLine("<td></td>");
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");
            File.WriteAllText($@"{dizin}\{dosyaismi}.htm", builder.ToString(), Encoding.UTF8);
        }

        public static void GetAtYarisi(List<UsersBudget> atyarisi)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<!doctype html>");
            builder.AppendLine("<html>");
            builder.AppendLine("<head>");
            builder.AppendLine("<title> At Yarışı </title>");
            builder.AppendLine("<meta charset = \"utf -8\"/>");
            builder.AppendLine("<meta http-equiv=\"refresh\" content=\"20\" />");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            if (GlobalVars.btc50up)
                builder.AppendLine("<b><p style=\"color:green\">Piyasa YEŞİL</p></b>");
            else
                builder.AppendLine("<b><p style=\"color:red\">Piyasa KIRMIZI</p></b>");
            if (GlobalVars.btc20up)
                builder.AppendLine("<b><p style=\"color:green\">BTC MA20 üstü</p></b>");
            else
                builder.AppendLine("<b><p style=\"color:red\">BTC MA20 altı</p></b>");
            //builder.AppendLine("<br>");
            builder.AppendLine($"<b>Güncelleme zamanı : {DateTime.Now.ToString()}</b>");
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine("<tr>");
            builder.AppendLine("<td>");
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine($"<tr><th colspan=\"2\">Bütçeler Savaşı</th></tr>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Bütçe</th>");
            builder.AppendLine("<th>Toplam Para</th>");
            builder.AppendLine("</tr>");
            foreach (var item in atyarisi)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine($"<td><a href=\"{item.HtmlHeader}\">{item.UserName}</a></td>");   //1
                builder.AppendLine($"<td>{item.RemainingMoney.ToString("F3")}</td>");    //2
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            builder.AppendLine("</td>");

            builder.AppendLine("<td>");
            builder.AppendLine("<table border = 1 >");
            builder.AppendLine($"<tr><th colspan=\"2\">Bütçeler Savaşı</th></tr>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Bütçe</th>");
            builder.AppendLine("<th>Net Kar(%)</th>");
            builder.AppendLine("</tr>");
            atyarisi = atyarisi.OrderByDescending(x => x.NetWorth).ToList();
            foreach (var item in atyarisi)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine($"<td><a href=\"{item.HtmlHeader}\">{item.UserName}</a></td>");   //1
                builder.AppendLine($"<td>{item.NetWorth.ToString("F2")}</td>");    //2
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            builder.AppendLine("</td>");
            builder.AppendLine("</tr>");
            builder.AppendLine("</table>");


            builder.AppendLine("</body>");
            builder.AppendLine("</html>");
            File.WriteAllText($@"{dizin}\atyarisi.htm", builder.ToString(), Encoding.UTF8);
        }
    }
}
