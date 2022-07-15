using Binance.Mysql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Binance.Connections;
using Binance.Commons;
namespace Telegram.Client
{
    // ema alsat -527539110
    public class TelegramBot
    {
        string tokenID = "1671579207:AAFiW8aML8Gsm4J8wMIdKg-JHqpIqXK6OZM";
        private TelegramBotClient bot;
        object lockobj = new object();
//        public long emaalsat_chat_id = -1001302125605;

        public event EventHandler<TelegramMessageEventArgs> MessageReceived;
        public TelegramBot()
        {
            bot = new TelegramBotClient(tokenID);
            if (!GlobalVars.Istest)
            {
                bot.StartReceiving();
                bot.OnMessage += Bot_OnMessage;
            }
        }

        protected virtual void OnMessageReceived(TelegramMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }
        private void Bot_OnMessage(object sender, Bot.Args.MessageEventArgs e)
        {
            Debug.WriteLine($"Mesaj alındı: {e.Message.Text}");
            string okunan = e.Message.Text;
            if (okunan == null)
                return;
            OnMessageReceived(new TelegramMessageEventArgs() { UserId = e.Message.From.Id, ChatId = e.Message.Chat.Id, Messages = e.Message.Text });
            return;
        }

        public bool sendMessage(string message, long channelId)// = -515717409)
        {
            lock (lockobj)
            {
                try
                {
                    bot.SendTextMessageAsync(channelId, message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
                return true;
            }

        }

    }
}
