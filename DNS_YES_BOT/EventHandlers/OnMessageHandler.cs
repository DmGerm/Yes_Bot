using DNS_YES_BOT.RouteTelegramData;
using DNS_YES_BOT.ShopService;
using DNS_YES_BOT.UserService;
using DNS_YES_BOT.VoteService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnMessageHandler(TelegramBotClient telegramBotClient, IShopRepo shopRepo, IVoteService voteService, IAdminRepo adminRepo, IRouteData routeData)
    {
        private readonly TelegramBotClient _botClient = telegramBotClient;
        private readonly IShopRepo _shopRepo = shopRepo;
        private readonly IVoteService _voteService = voteService;
        private readonly IAdminRepo _userRepo = adminRepo;
        private readonly IRouteData _routeData = routeData;

        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.From is null || msg.Text is null)
            {
                return;
            }
            var command = msg.Text.Split('@')[0];

            if (command == "/start")
            {
                await StartNewVote(msg);
            }

            if (command == "/results")
            {
                await ShowVoteResults(msg);
            }

            if (command == "/add_admin")
            {
                await HandleAddAdmin(msg);
            }

            if (command == "/admin_service")
            {
                await ShowAdminPanel(msg);
            }
        }

        private async Task<Message> ShowAdminPanel(Message msg)
        {
            if (msg.From is null)
                throw new ArgumentNullException(nameof(msg), "Message.From cannot be null.");

            if (msg.Chat.Type != ChatType.Private)
                await _botClient.SendMessage(msg.Chat.Id, "Данная команда доступна только в личных чатах.");

            if (!await _userRepo.UserIdExistsAsync(msg.From.Id))
                return await _botClient.SendMessage(msg.Chat.Id, "Вы не являетесь администратором!");

            return await _botClient.SendMessage(
                           msg.Chat.Id,
                           "Выберите действие:",
                           replyMarkup: new InlineKeyboardMarkup(
                           [
                            [
                    InlineKeyboardButton.WithCallbackData("Добавить магазин", "shop_add"),
                    InlineKeyboardButton.WithCallbackData("Добавить сотрудника", "employee_add")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("Вывести список магазинов", "shops_show"),
                    InlineKeyboardButton.WithCallbackData("Удалить магазин", "shop_del")
                ]
                           ]));
        }

        private async Task StartNewVote(Message msg)
        {
            if (await _voteService.CheckEntity(msg.Chat.Id))
            {
                await _botClient.SendMessage(msg.Chat.Id, "Голосование в этом чате уже было проведено");
                return;
            }

            var shops = await _shopRepo.GetShopsAsync();

            var buttons = shops
                .Select(shop => InlineKeyboardButton.WithCallbackData(
                    shop.ShopName,
                    $"vote_{shop.ShopName}"))
                .Select((button, index) => new { button, index })
                .GroupBy(x => x.index / 1)  
                .Select(group => group.Select(x => x.button).ToList())
                .ToList();

            await _botClient.SendMessage(
                msg.Chat.Id,
                "Нажмите кнопку вашего магазина, чтобы подтвердить ознакомление с информацией:",
                replyMarkup: new InlineKeyboardMarkup(buttons));
        }

        private async Task ShowVoteResults(Message msg)
        {
            if (msg is null || msg.From is null)
                throw new Exception("Message or From is null");

            if (!await _userRepo.UserIdExistsAsync(msg.From.Id))
            {
                await _botClient.SendMessage(msg.Chat.Id, "Вы не являетесь администратором!");
                return;
            }

            var results = await _voteService.GetResultsAsync(msg.Chat.Id);
            var url = await _routeData.GetVoteUrlAsync(results);

            await _botClient.SendMessage(msg.From.Id, $"Результаты голосования (ссылка действительна 30 минут:\n{url}");
        }

        private async Task HandleAddAdmin(Message message)
        {
            if (message.From == null)
                throw new ArgumentNullException(nameof(message), "CallbackQuery.Message cannot be null.");

            if (await _userRepo.UserListIsEmptyAsync())
            {
                await _userRepo.AddAdminAsync(message.From.Id);
                await _botClient.SendMessage(message.Chat.Id, "Вы стали первым администратором бота.");
                return;
            }

            if (!await _userRepo.UserIdExistsAsync(message.From.Id))
            {
                await _botClient.SendMessage(message.Chat.Id, "Вы не являетесь администратором!");
                return;
            }

            var administrators = await _botClient.GetChatAdministrators(message.Chat.Id);
            if (administrators.Length != 0)
            {
                var inlineKeyboard = new InlineKeyboardMarkup(
                    administrators.Select(admin =>
                    {
                        var user = admin.User;
                        return InlineKeyboardButton.WithCallbackData(user?.Username ?? "Unknown User", $"add_admin_{user?.Id}");
                    }));

                await _botClient.SendMessage(message.Chat.Id,
                    "Выберите пользователя для добавления в список администраторов:",
                    replyMarkup: inlineKeyboard);
            }
            else
            {
                await _botClient.SendMessage(message.Chat.Id, "В чате нет администраторов.");
            }
        }
    }

}
