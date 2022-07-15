using Binance.Commons;
using Binance.Mysql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Client;

namespace Binance.Forms
{
    public class UserWorkers
    {
        public List<UserWorker> userWorkers { get; set; }
        private MysqlClass mysqlClass;
        public List<User> Users;
        public UserWorkers(List<User> users, TelegramBot telegramBot)
        {
            userWorkers = new List<UserWorker>();
            mysqlClass = new MysqlClass();
            Users = users;
            foreach (var item in Users)
            {
                userWorkers.Add(new UserWorker(item, telegramBot));
            }
        }

        public UserWorker this[long userid]
        {
            get
            {
                foreach (var item in userWorkers)
                {
                    if (item.user.Userid == userid)
                        return item;
                }
                return null;
            }
        }
        public UserWorker this[string username]
        {
            get
            {
                foreach (var item in userWorkers)
                {
                    if (item.user.UserName == username)
                        return item;
                }
                return null;

            }
        }
        public void DoBuyOrSell(List<NewBuyTable> alimlist, List<NewBuyTable> satimlist,string sure)
        {
            foreach (var item in userWorkers)
            {
                //item.DoBuyOrSell(alimlist, tumlist);
                Task task = new Task(() => item.DoBuyOrSell(alimlist, satimlist, sure));
                task.Start();
            }
        }

        internal void TargetReached(bool systemStatusNormal)
        {
            Task[] tasks = new Task[userWorkers.Count];
            int sira = 0;
            foreach (var item in userWorkers)
            {
                Task task = new Task(() => item.TargetReached( systemStatusNormal));
                task.Start();
                tasks[sira++] = task;
            }
            Task.WaitAll(tasks.ToArray());
            List<UsersBudget> allbudgets = new List<UsersBudget>();
            foreach (var item in userWorkers)
            {
                allbudgets.AddRange(item.AtYarisiBudget);
            }
            HtmlHelper.GetAtYarisi(allbudgets.OrderByDescending(x => x.RemainingMoney).ToList());
        }
        internal void AutobotControl()
        {
            Task[] tasks = new Task[userWorkers.Count];
            int sira = 0;
            foreach (var item in userWorkers)
            {
                Task task = new Task(() => item.AutobotControl());
                task.Start();
                tasks[sira++] = task;
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
