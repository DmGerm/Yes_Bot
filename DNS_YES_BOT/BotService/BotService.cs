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

            OnMessageHandler messageHandler = new(bot, _shopRepo, _voteService, _adminRepo);
            OnUpdateHandler onUpdateHandler = new(bot, _adminRepo, _shopRepo, _voteService);

            var me = await bot.GetMe();

            bot.OnError += OnErrorHandler.OnError;
            bot.OnMessage += messageHandler.OnMessage;
            bot.OnUpdate += onUpdateHandler.OnUpdate;

            await SetBotCommandsAsync(bot);

            Console.WriteLine($"@{me.Username} Запущен... Нажмите Enter для остановки бота.");
            Console.ReadLine();
            cts.Cancel();
        }

        private static async Task SetBotCommandsAsync(ITelegramBotClient botClient)
        {
            ArgumentNullException.ThrowIfNull(botClient);

            var privateCommands = new List<BotCommand>
                  {
                 new() { Command = "admin_service", Description = "Начать работу с ботом" },
                 new() { Command = "help", Description = "Получить помощь" },
                  };

            var groupCommands = new List<BotCommand>
                  {
                      new() { Command = "start", Description = "Начать голосование" },
                      new() { Command = "results", Description = "Посмотреть результаты голосования" },
                      new() { Command = "add_admin", Description = "Добавить администратора" }
                  };

            try
            {
                await botClient.SetMyCommands(
                    privateCommands,
                    new BotCommandScopeAllPrivateChats()
                );

                await botClient.SetMyCommands(
                    groupCommands,
                    new BotCommandScopeAllGroupChats()
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to set bot commands", ex);
            }
        }
    }
}