using DNS_YES_BOT.EventHandlers;
using DNS_YES_BOT.UserService;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DNS_YES_BOT.Service
{
    public class BotService(string botToken)
    {
        private readonly string _botToken = botToken;
        private readonly IUserRepo _userRepo = new UserRepo();
        public async Task BotRun()
        {

            using var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(_botToken, cancellationToken: cts.Token);

            OnMessageHandler messageHandler = new(bot);
            OnUpdateHandler onUpdateHandler = new(bot);

            var me = await bot.GetMe();

            bot.OnError += OnErrorHandler.OnError;
            bot.OnMessage += messageHandler.OnMessage;
            bot.OnUpdate += onUpdateHandler.OnUpdate;

            await SetBotCommandsAsync(bot);

            Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
            Console.ReadLine();
            cts.Cancel();
        }

        async Task SetBotCommandsAsync(ITelegramBotClient botClient)
        {
            var commands = new[]
            {
                new BotCommand { Command = "start", Description = "Запустить сбор информации" },
                new BotCommand { Command = "enter", Description = "Авторизация в боте" },
                //new BotCommand { Command = "info", Description = "Получить информацию о боте" },
            };

            await botClient.SetMyCommands(commands);
        }
    }
}