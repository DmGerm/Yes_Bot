using DNS_YES_BOT.EventHandlers;
using DNS_YES_BOT.RouteTelegramData;
using DNS_YES_BOT.ShopService;
using DNS_YES_BOT.UserService;
using DNS_YES_BOT.VoteService;
using System.Runtime.Loader;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DNS_YES_BOT.BotService
{
    public class BotService(string botToken)
    {
        private readonly string _botToken = botToken;
        private readonly IAdminRepo _adminRepo = new AdminRepo();
        private IShopRepo? _shopRepo;
        private readonly IVoteService _voteService = new VoteServiceRe();
        private IRouteData? _routeData;
        private bool _isDisposed = false;
        public async Task BotRun()
        {
            using var cts = new CancellationTokenSource();
            _routeData = new RouteData(cts.Token);
            _shopRepo = new ShopRepo(_routeData);
            var bot = new TelegramBotClient(_botToken, cancellationToken: cts.Token);
            var sendDataTask = Task.Run(async () =>
            {
                await Task.Delay(30000);
                await _routeData.SendDataOnceAsync(await _shopRepo.GetShopNamesAsync());
            });

            AssemblyLoadContext.Default.Unloading += ctx =>
             {
                 if (!_isDisposed)
                 {
                     Console.WriteLine("Получен сигнал завершения.");
                     cts.Cancel();
                 }
             };

            Console.CancelKeyPress += (_, e) =>
            {
                Console.WriteLine("Завершение работы...");
                e.Cancel = true;
                cts.Cancel();
            };

            OnMessageHandler messageHandler = new(bot, _shopRepo, _voteService, _adminRepo, _routeData);
            OnUpdateHandler onUpdateHandler = new(bot, _adminRepo, _shopRepo, _voteService);

            var me = await bot.GetMe();

            bot.OnError += OnErrorHandler.OnError;
            bot.OnMessage += messageHandler.OnMessage;
            bot.OnUpdate += onUpdateHandler.OnUpdate;

            Console.WriteLine($"@{me.Username} Запущен... ");

            try
            {
                await SetBotCommandsAsync(bot);

                if (OperatingSystem.IsWindows())
                {
                    Console.WriteLine("Нажмите Enter для завершения работы...");
                    Console.ReadLine();
                    cts.Cancel();
                }
                else
                {
                    Console.WriteLine("Ожидаем завершения через Ctrl+C...");
                    AssemblyLoadContext.Default.Unloading += _ => cts.Cancel();
                    Console.CancelKeyPress += (_, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                    };
                    await Task.Delay(Timeout.Infinite, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Операция отменена.");
            }
            finally
            {
                Console.WriteLine("Завершаем работу...");
                bot.OnMessage -= messageHandler.OnMessage;
                bot.OnUpdate -= onUpdateHandler.OnUpdate;

                cts.Dispose();
                _routeData.Dispose();
                _isDisposed = true;
                Console.WriteLine("Бот успешно остановлен.");
            }
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