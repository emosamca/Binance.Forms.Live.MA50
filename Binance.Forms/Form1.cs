//#define DEBUGMODE
using Binance.Commons;
using Binance.Connections;
using Binance.Indicator;
using Binance.Mysql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Client;

namespace Binance.Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void start()
        {
            var mysqlClass = new MysqlClass();
            var users = mysqlClass.GetUsers();
            GlobalVars.Istest = mysqlClass.IsTest();

            var bot = new TelegramBot();
            //if (GlobalVars.Istest)
            //    Debug.WriteLine("Test ortamındayız.");
            //else
                Debug.WriteLine("Aman dikkat. Başladık.");
            users = users.FindAll(x => x.UserName == "emrah");
            var userWorkers = new UserWorkers(users, bot);
            var analizWorker = new AnalizWorker(userWorkers, bot);
            analizWorker.StartAnaliz();
            Debug.WriteLine("Ana thread bitti");
            Debug.WriteLine("Ana thread şimdi bitti");

        }
        private void button1_Click(object sender, EventArgs e)
        {
            start();
        }

        private void btnReconnect_Click(object sender, EventArgs e)
        {
            var mysqlClass = new MysqlClass();
            var users = mysqlClass.GetUsers();
            GlobalVars.Istest = mysqlClass.IsTest();

            var bot = new TelegramBot();
            //if (GlobalVars.Istest)
            //    Debug.WriteLine("Test ortamındayız.");
            //else
                Debug.WriteLine("Aman dikkat. Tekrar başladık.");
            var userWorkers = new UserWorkers(users, bot);
            var analizWorker = new AnalizWorker(userWorkers, bot);
            analizWorker.LMTSTaramaTask();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var s = JObject.Parse( textBox1.Text);
            var m = s["e"].ToString();
            var o = s["o"];
            Debug.WriteLine(o.ToString());
            var sonuc2 = JsonConvert.DeserializeObject<futureOrderDetails>(o.ToString());
            foreach (var item in o.Values())
            {
                Debug.WriteLine(item.ToString());
            }
            var sonuc = JsonConvert.DeserializeObject<futuredatauserstream>(textBox1.Text);


            var mysqlClass = new MysqlClass();
            var users = mysqlClass.GetUsers();
            GlobalVars.Istest = mysqlClass.IsTest();

            var bot = new TelegramBot();
            //if (GlobalVars.Istest)
            //    Debug.WriteLine("Test ortamındayız.");
            //else
            Debug.WriteLine("Aman dikkat. Başladık.");
            users = users.FindAll(x => x.UserName == "emrah");
            var userWorkers = new UserWorkers(users, bot);
            var analizWorker = new AnalizWorker(userWorkers, bot);
            var trades= userWorkers.userWorkers.First().binanceServices.GetTradeAsync("RSRBUSD");

        }
    }
}
