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
        private readonly VoteManager _votemanager = new VoteManager(voteService, telegramBotClient, shopRepo, adminRepo);

        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.From is null || msg.Text is null) return;

            var command = msg.Text.Split('@')[0];

            var commands = new Dictionary<string, Func<Task>>
            {
                ["/start"] = () => Task.Run(() => _votemanager.StartVoteAsync(msg)),
                ["/results"] = () => ShowVoteResults(msg),
                ["/add_admin"] = () => HandleAddAdmin(msg),
                ["/admin_service"] = () => ShowAdminPanel(msg),
                ["/show_admins"] = () => ShowAdmins(msg),
                ["/cancel_vote"] = () => _votemanager.EndVoteAsync(msg),
                ["/help"] = () => ShowHelp(msg)
            };

            if (commands.TryGetValue(command, out var action))
            {
                await action();
            }
        }

        private async Task<Message> ShowAdmins(Message msg)
        {
            if (msg.From is null)
                throw new ArgumentNullException(nameof(msg), "Message.From cannot be null.");


            if (msg.Chat.Type != ChatType.Private)
            {
                var me = await _botClient.GetMe();
                var button = InlineKeyboardButton.WithUrl("Написать боту", $"https://t.me/{me.Username}");
                await SendMessageToChannel(msg, "Команда доступна только в личном чате с ботом.");
                return await Task.FromResult(msg);
            }
            var adminIds = await _userRepo.GetAllUserIdsAsync();

            var adminsNames = await Task.WhenAll(adminIds.Select(async id =>
            {
                try
                {
                    var user = await _botClient.GetChat(id);
                    return !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName)
                        ? $"{user.FirstName} {user.LastName}"
                        : !string.IsNullOrEmpty(user.FirstName)
                            ? user.FirstName
                            : !string.IsNullOrEmpty(user.LastName)
                                ? user.LastName
                                : id.ToString();
                }
                catch
                {
                    return id.ToString();
                }
            }));

            return await _botClient.SendMessage(msg.Chat.Id, String.Join("\n", adminsNames));
        }

        private async Task ShowHelp(Message msg) => await SendMessageToChannel(msg, "Для начала голосования введите команду /start\n" +
                          "Для просмотра результатов голосования введите команду /results - необходимо иметь личный диалог с ботом\n" +
                          "Для управления ботом введите /admin_service - в личных сообщениях");

        private async Task<Message> ShowAdminPanel(Message msg)
        {
            if (msg.From is null)
                throw new ArgumentNullException(nameof(msg), "Message.From cannot be null.");


            if (msg.Chat.Type != ChatType.Private)
            {
                var me = await _botClient.GetMe();
                var button = InlineKeyboardButton.WithUrl("Написать боту", $"https://t.me/{me.Username}");
                await SendMessageToChannel(msg, "Команда доступна только в личном чате с ботом.");
                return await Task.FromResult(msg);
            }
            else if (!await _userRepo.UserIdExistsAsync(msg.From.Id))
            {
                return await _botClient.SendMessage(msg.Chat.Id, "Вы не являетесь администратором!");
            }
            else
            {
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
        }

        private async Task ShowVoteResults(Message msg)
        {
            if (msg is null || msg.From is null)
                throw new Exception("Message or From is null");

            if (!await _userRepo.UserIdExistsAsync(msg.From.Id))
            {
                await SendMessageToChannel(msg, "Вы не являетесь администратором!");
                return;
            }

            var results = await _voteService.GetResultsAsync(msg.MessageThreadId ?? msg.Chat.Id);
            var shops = await _shopRepo.GetShopsAsync();
            var shopNames = shops.Select(shop => shop.ShopName).ToList();
            var votedShops = results.VoteResults.Where(x => shopNames.Contains(x.Key)).Select(x => x.Key).ToList();
            var url = await _routeData.GetVoteUrlAsync(results);
            try
            {
                if (votedShops.Count == shopNames.Count)
                    await SendMessageToChannel(msg, $"Все магазины проголосовали!");

                await _botClient.SendMessage(msg.From.Id, $"Результаты голосования:\n<a href=\"{url}/\">Нажмите для просмотра</a>", ParseMode.Html);
            }
            catch
            {
                var me = await _botClient.GetMe();
                var button = InlineKeyboardButton.WithUrl("Написать боту", $"https://t.me/{me.Username}");
                var inlineKeyboard = new InlineKeyboardMarkup(button);
                await SendMessageToChannelWithReplyMarkup(msg, "Команда доступна после старта личного диалога с ботом.", inlineKeyboard);
            }
        }

        private async Task HandleAddAdmin(Message message)
        {
            if (message.From == null)
                throw new ArgumentNullException(nameof(message), "CallbackQuery.Message cannot be null.");

            if (await _userRepo.UserListIsEmptyAsync())
            {
                await _userRepo.AddAdminAsync(message.From.Id);
                await SendMessageToChannel(message, "Вы стали первым администратором бота.");
                return;
            }

            if (!await _userRepo.UserIdExistsAsync(message.From.Id))
            {
                await SendMessageToChannel(message, "Вы не являетесь администратором!");
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

                await SendMessageToChannelWithReplyMarkup(message, "Выберите пользователя для добавления в список администраторов:", inlineKeyboard);
            }
            else
            {
                await SendMessageToChannel(message, "В чате нет администраторов.");
            }
        }

        private async Task SendMessageToChannel(Message msg, string messageText)
        {
            if (msg.MessageThreadId != null)
            {
                await _botClient.SendMessage(
                    chatId: msg.Chat.Id,
                    replyParameters: msg.MessageId,
                    text: messageText
                    );
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: msg.Chat.Id,
                    text: messageText);
            }
        }

        private async Task SendMessageToChannelWithReplyMarkup(Message msg, string messageText, InlineKeyboardMarkup inlineKeyboard)
        {
            if (msg.MessageThreadId != null)
            {
                await _botClient.SendMessage(
                    chatId: msg.Chat.Id,
                    text: messageText,
                    replyParameters: msg.MessageId,
                    replyMarkup: inlineKeyboard);
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: msg.Chat.Id,
                    text: messageText, replyMarkup: inlineKeyboard);
            }
        }
    }
}
