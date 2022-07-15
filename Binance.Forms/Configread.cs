using Binance.Commons;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Forms
{
    public class Configread
    {
        public Configread()
        {

        }
        public List<UserInfo> GetUserInfos()
        {
            List<UserInfo> liste = new List<UserInfo>();

            var applicationSettings = ConfigurationManager.GetSection("ApplicationSettings") as NameValueCollection;
            if (applicationSettings.Count == 0)
            {
                Console.WriteLine("Application Settings are not defined");
            }
            else
            {
                foreach (var key in applicationSettings.AllKeys)
                {
                    if (key.StartsWith("user."))
                    {
                        string name = key.Split('.').Last();
                        UserInfo user = new UserInfo();
                        user.UserName = name;
                        string[] listem = applicationSettings[key].Split(',');
                        user.Key = listem[0];
                        user.SecretKey = listem[1];
                        user.GrupId = Convert.ToInt64(listem[2]);
                        user.UserId = Convert.ToInt64(listem[3]);
                        liste.Add(user);
                    }
                }
            }
            return liste;
        }

    }
}
