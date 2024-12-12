using DNS_YES_BOT.EventHandlers;
using DNS_YES_BOT.ShopService;
using DNS_YES_BOT.UserService;
using DNS_YES_BOT.VoteService;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DNS_YES_BOT.BotService
{
    public class BotService(string botToken)
    {
        private readonly string _botToken = botToken;
        private readonly IAdminRepo _adminRepo = new AdminRepo();
        private readonly IShopRepo _shopRepo = new ShopRepo();
        private readonly IVoteService _voteService = new VoteServiceRe();
        public async Task BotRun()
        {

            using var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(_botToken, cancellationToken: cts.Token);

            OnMessageHandler messageHandler = new(bot, _shopRepo, _voteService);
            OnUpdateHandler onUpdateHandler = new(bot, _adminRepo, _shopRepo, _voteService);

            var me = await bot.GetMe();

            bot.OnError += OnErrorHandler.OnError;
            bot.OnMessage += messageHandler.OnMessage;
            bot.OnUpdate += onUpdateHandler.OnUpdate;

            await SetBotCommandsAsync(bot);

            Console.WriteLine($"@{me.Username} Урок 9. Лекция Безопасная разработка приложенийis running... Press Enter to terminate");
            Console.ReadLine();
            cts.Cancel();
        }

        private static async Task SetBotCommandsAsync(ITelegramBotClient botClient)
        {
            ArgumentNullException.ThrowIfNull(botClient);

            var commands = new List<BotCommand>
                  {
                       new() { Command = "authorize", Description = "Авторизоваться" },
                       new() { Command = "start", Description = "Запустить сбор информации" },
                       new() { Command = "admin_service", Description = "Панель управления" }
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