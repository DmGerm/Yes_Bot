using DNS_YES_BOT.ShopService;
using DNS_YES_BOT.UserService;
using DNS_YES_BOT.VoteService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnUpdateHandler(TelegramBotClient telegramBotClient, IAdminRepo userRepo, IShopRepo shopRepo, IVoteService voteService)
    {
        private readonly TelegramBotClient _botClient = telegramBotClient;
        private readonly IAdminRepo _userRepo = userRepo;
        private readonly IShopRepo _shopRepo = shopRepo;
        private readonly IVoteService _voteService = voteService;
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
                    { "shop_del", HandleShopDelete },
                    { "shops_show", HandleShopsShow },
                    { "employee_add", query => _botClient.AnswerCallbackQuery(query.Id, "Функция добавления сотрудника пока не реализована.") }
                 };

            if (data.StartsWith("add_admin"))
            {
                await HandleAddAdminCallback(query, data);
            }
            else if (data.StartsWith("vote_"))
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
            if (query.Message is null || query.Message.From is null)
                throw new Exception("Message or From is null");

            data.Split('_');
            var shopName = data.Split('_')[1];

            await _voteService.AddEntity(query.Message.Chat.Id, shopName, String.Concat(query.From.FirstName, " ", query.From.LastName));
            await _botClient.AnswerCallbackQuery(query.Id, "Голос зачтен");
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
            await _botClient.SendMessage(query.Message.Chat.Id, $"Список магазинов:\n{shopsString}");
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
                        ReplaceOnMessageHandler(null);
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
            if (query.Message is not null)
            {
                if (!await _userRepo.UserIdExistsAsync(id) && query is not null)
                {
                    await _userRepo.AddAdminAsync(id);
                    if (query.Message.ReplyToMessage != null)
                    {
                        await _botClient.SendMessage(
                            query.Message!.Chat,
                            text: $"Пользователь {query.From.Username} добавил нового администратора с ID {id}.",
                            replyParameters: query.Message.ReplyToMessage.MessageId);
                    }
                    else
                    {
                        await _botClient.SendMessage(
                            query.Message.Chat,
                            text: $"Пользователь {query.From.Username} добавил нового администратора с ID {id}.");
                    }
                }
                else
                {
                    if (query.Message.Chat.Type is ChatType.Supergroup)
                    {
                        await _botClient.SendMessage(
                            query.Message!.Chat,
                            text: $"Пользователь {query.From.Username} пытается добавить администратора с ID {id}, но он уже существует!",
                            replyParameters: query.Message.ReplyToMessage.MessageId);
                    }
                    else
                    {
                        await _botClient.SendMessage(
                            query.Message.Chat,
                            text: $"Пользователь {query.From.Username} пытается добавить администратора с ID {id}, но он уже существует!");
                    }
                }
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