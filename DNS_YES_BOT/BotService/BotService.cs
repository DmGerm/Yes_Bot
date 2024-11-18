using DNS_YES_BOT.EventHandlers;
using DNS_YES_BOT.UserService;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DNS_YES_BOT.BotService
{
    public class BotService(string botToken)
    {
        private readonly string _botToken = botToken;
        private readonly IUserRepo _userRepo = new UserRepo();
        public async Task BotRun()
        {

            using var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(_botToken, cancellationToken: cts.Token);

            OnMessageHandler messageHandler = new(bot, _userRepo);
            OnUpdateHandler onUpdateHandler = new(bot, _userRepo);

            var me = await bot.GetMe();

            bot.OnError += OnErrorHandler.OnError;
            bot.OnMessage += messageHandler.OnMessage;
            bot.OnUpdate += onUpdateHandler.OnUpdate;

            await SetBotCommandsAsync(bot);

            Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
            Console.ReadLine();
            cts.Cancel();
        }

        private async Task SetBotCommandsAsync(ITelegramBotClient botClient)
        {
            if (botClient == null)
                throw new ArgumentNullException(nameof(botClient));

            var commands = new List<BotCommand>
                  {
                       new() { Command = "authorize", Description = "Авторизоваться" },
                       new() { Command = "start", Description = "Запустить сбор информации" },
                       new() { Command = "add_admin", Description = "Добавить администратора" }
                  };

            try
            {
                await botClient.SetMyCommands(commands);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to set bot commands", ex);
            }
        }
    }
}