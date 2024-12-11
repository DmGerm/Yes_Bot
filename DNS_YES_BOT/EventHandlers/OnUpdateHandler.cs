using DNS_YES_BOT.ShopService;
using DNS_YES_BOT.UserService;
using DNS_YES_BOT.VoteService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnUpdateHandler(TelegramBotClient telegramBotClient, IAdminRepo userRepo, IShopRepo shopRepo, IVoteService voteService)
    {
        private readonly TelegramBotClient _botClient = telegramBotClient;
        private readonly IAdminRepo _userRepo = userRepo;
        private readonly IShopRepo _shopRepo = shopRepo;
        private readonly IVoteService _voteService;
        private TelegramBotClient.OnMessageHandler? _onMessageHandler;
        private Dictionary<string, Func<CallbackQuery, Task>>? _callbackHandlers;
        public async Task OnUpdate(Update update)
        {
            if (update.CallbackQuery is { Data: { } data } query)
            {   
                await HandleCallbackQuery(query, data);
            }
            else if (update.Type == UpdateType.ChatMember && update.ChatMember?.NewChatMember.User.Id == _botClient.BotId)
            {
                await HandleChatMemberUpdate(update.ChatMember);
            }
        }

        private async Task HandleCallbackQuery(CallbackQuery query, string data)
        {
            _callbackHandlers = new Dictionary<string, Func<CallbackQuery, Task>>
                 {
                    { "shop_add", HandleAddShop },
                    { "admin_add", HandleAddAdmin  },
                    { "shop_del", HandleShopDelete },
                    { "shops_show", HandleShopsShow },
                    { "employee_add", query => _botClient.AnswerCallbackQuery(query.Id, "Функция добавления сотрудника пока не реализована.") }
                 };

            if (data.StartsWith("add_admin"))
            {
                await HandleAddAdminCallback(query, data);
            }
            else if (data.StartsWith("shop_"))
            {
                await HandleVoteResult(query, data);
            }
            else if (_callbackHandlers.ContainsKey(data))
            {
                await _callbackHandlers[data](query);
            }
            else
            {
                await _botClient.AnswerCallbackQuery(query.Id, "Неизвестное действие.");
            }
        }

        private async Task HandleVoteResult(CallbackQuery query, string data)
        {
            var shopId = data.Split('_')[1];
            _voteService.AddEntity(query.Message.Chat.Id, )
        }

        private async Task HandleShopsShow(CallbackQuery query)
        {
            if (query.Message == null)
            {
                await _botClient.AnswerCallbackQuery(query.Id, "Ошибка: сообщение не найдено.");
                return;
            }

            await _botClient.AnswerCallbackQuery(query.Id, "Вы выбрали просмотр магазинов.");
            var shops = await _shopRepo.GetShopsAsync();
            var shopNames = shops.Select(shop => shop.ShopName).ToList();

            if (shopNames.Count == 0)
            {
                await _botClient.SendMessage(query.Message.Chat.Id, "Список магазинов пуст.");
                return;
            }

            var shopsString = string.Join(",\n", shopNames);
            await _botClient.SendMessage(query.Message.Chat.Id, $"Список магазинов:\n {shopsString}");
        }

        private async Task HandleShopDelete(CallbackQuery query)
        {
            if (query.Message == null)
            {
                await _botClient.AnswerCallbackQuery(query.Id, "Ошибка: сообщение не найдено.");
                return;
            }

            await _botClient.SendMessage(query.Message.Chat.Id, "Введите название магазина:");

            ReplaceOnMessageHandler(async (message, updateType) =>
            {
                if (message.Type == MessageType.Text)
                {
                    if (string.IsNullOrWhiteSpace(message.Text))
                    {
                        await _botClient.SendMessage(query.Message.Chat.Id, "Название магазина не может быть пустым. Попробуйте ещё раз.");
                        return;
                    }

                    if (!await _shopRepo.IsShopExistAsync(message.Text))
                    {
                        await _botClient.SendMessage(query.Message.Chat.Id, $"Магазин с названием \"{message.Text}\" не существует в базе.");
                    }
                    else
                    {
                        await _shopRepo.RemoveShopAsync(message.Text);
                        await _botClient.SendMessage(query.Message.Chat.Id, $"Магазин \"{message.Text}\" успешно удален.");
                        ReplaceOnMessageHandler(null);
                    }
                }
            });
        }

        private async Task HandleAddAdminCallback(CallbackQuery query, string data)
        {
            await _botClient.AnswerCallbackQuery(query.Id, $"Вы выбрали {data}");

            var parts = data.Split('_');
            if (parts.Length != 3 || !long.TryParse(parts[2], out var id))
            {
                await _botClient.SendMessage(query.Message!.Chat, "Некорректный формат команды.");
                return;
            }

            if (!await _userRepo.UserIdExistsAsync(id))
            {
                await _userRepo.AddAdminAsync(id);
                await _botClient.SendMessage(query.Message!.Chat,
                    $"Пользователь {query.From.Username} добавил нового администратора с ID {id}.");
            }
            else
            {
                await _botClient.SendMessage(query.Message!.Chat,
                    $"Пользователь {query.From.Username} пытается добавить администратора с ID {id}, но он уже существует!");
            }
        }

        private async Task HandleChatMemberUpdate(ChatMemberUpdated chatMember)
        {
            var addedByUserId = chatMember.From.Id;

            if (!await _userRepo.UserIdExistsAsync(addedByUserId))
            {
                Console.WriteLine($"Бот добавлен в группу авторизованным пользователем: {chatMember.From.Username}");
            }
            else
            {
                await _botClient.LeaveChat(chatMember.Chat.Id);
                Console.WriteLine($"Бот покинул группу, добавивший пользователь: {chatMember.From.Username}");
            }
        }

        private async Task HandleAddAdmin(CallbackQuery query)
        {
            if (query.Message == null)
                throw new ArgumentNullException(nameof(query.Message), "CallbackQuery.Message cannot be null.");

            if (await _userRepo.UserListIsEmptyAsync())
            {
                await _userRepo.AddAdminAsync(query.From.Id);
                await _botClient.SendMessage(query.Message.Chat.Id, "Вы стали первым администратором бота.");
                return;
            }

            if (!await _userRepo.UserIdExistsAsync(query.From.Id))
            {
                await _botClient.SendMessage(query.Message.Chat.Id, "Вы не являетесь администратором!");
                return;
            }

            var administrators = await _botClient.GetChatAdministrators(query.Message.Chat.Id);
            if (administrators.Length != 0)
            {
                var inlineKeyboard = new InlineKeyboardMarkup(
                    administrators.Select(admin =>
                    {
                        var user = admin.User;
                        return InlineKeyboardButton.WithCallbackData(user?.Username ?? "Unknown User", $"add_admin_{user?.Id}");
                    }));

                await _botClient.SendMessage(query.Message.Chat.Id,
                    "Выберите пользователя для добавления в список администраторов:",
                    replyMarkup: inlineKeyboard);
            }
            else
            {
                await _botClient.SendMessage(query.Message.Chat.Id, "В чате нет администраторов.");
            }
        }
        private async Task HandleAddShop(CallbackQuery query)
        {
            if (query.Message == null)
            {
                await _botClient.AnswerCallbackQuery(query.Id, "Ошибка: сообщение не найдено.");
                return;
            }

            await _botClient.SendMessage(query.Message.Chat.Id, "Введите название магазина:");

            ReplaceOnMessageHandler(async (message, updateType) =>
            {
                if (message.Type == MessageType.Text)
                {
                    if (string.IsNullOrWhiteSpace(message.Text))
                    {
                        await _botClient.SendMessage(query.Message.Chat.Id, "Название магазина не может быть пустым. Попробуйте ещё раз.");
                        return;
                    }

                    if (await _shopRepo.IsShopExistAsync(message.Text))
                    {
                        await _botClient.SendMessage(query.Message.Chat.Id, $"Магазин с названием \"{message.Text}\" уже существует.");
                    }
                    else
                    {
                        await _shopRepo.AddShopAsync(message.Text);
                        await _botClient.SendMessage(query.Message.Chat.Id, $"Магазин \"{message.Text}\" успешно добавлен.");
                        ReplaceOnMessageHandler(null);
                    }
                }
            });
        }

        private void ReplaceOnMessageHandler(TelegramBotClient.OnMessageHandler? newHandler)
        {
            if (_onMessageHandler != null)
            {
                _botClient.OnMessage -= _onMessageHandler;
            }

            if (newHandler != null)
            {
                _onMessageHandler = newHandler;
                _botClient.OnMessage += _onMessageHandler;
            }
            else
            {
                _onMessageHandler = null;
            }
        }
    }
}