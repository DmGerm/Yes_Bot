using DNS_YES_BOT.EventHandlers;
using Telegram.Bot;

namespace DNS_YES_BOT.Service
{
    public class BotService(string botToken)
    {
        private readonly string _botToken = botToken;
        public async Task BotRun()
        {
            
            using var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(_botToken, cancellationToken: cts.Token);

            OnMessageHandler messageHandler = new OnMessageHandler(bot);
            OnUpdateHandler onUpdateHandler = new OnUpdateHandler(bot);

            var me = await bot.GetMe();

            bot.OnError += OnErrorHandler.OnError;
            bot.OnMessage += messageHandler.OnMessage;
            bot.OnUpdate += onUpdateHandler.OnUpdate;

            Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}
