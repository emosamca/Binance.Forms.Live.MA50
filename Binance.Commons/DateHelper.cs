using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Commons
{
    public static class DateHelper
    {
        public static string GetDateStrFromTimeStamp(long timestamp)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(timestamp);
            DateTime startdate = new DateTime(1970, 1, 1) + time;
            var datestr = startdate.ToLocalTime().ToString();
            return datestr;
        }
        public static long GetCurrentTimeStam()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        public static long GetTimeStampFromDate(DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
        }

        public static DateTime GetDateTimefromTimeStamp(long DateTimeStamp)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(DateTimeStamp);
            DateTime m_datetime= new DateTime(1970, 1, 1) + time;
            return m_datetime;

        }
        public static double GetDateDiff(long datestart, long dateend)
        {
            TimeSpan timeSpan = new TimeSpan((dateend - datestart)*10000);

            return timeSpan.TotalHours;
        }
        public static double GetDateDiffMin(long datestart, long dateend)
        {
            TimeSpan timeSpan = new TimeSpan((dateend - datestart) * 10000);

            return timeSpan.TotalMinutes;

        }
        public static long GetMinuteZeroCurrentTime()
        {
            var sonuc = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);       // DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var unix = ((DateTimeOffset)sonuc).ToUnixTimeMilliseconds();
            return unix;
        }
    }
}
