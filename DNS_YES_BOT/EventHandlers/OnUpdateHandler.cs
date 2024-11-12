using DNS_YES_BOT.UserService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnUpdateHandler(TelegramBotClient telegramBotClient, IUserRepo userRepo)
    {
        private readonly TelegramBotClient _botClient = telegramBotClient;
        private readonly IUserRepo _userRepo = userRepo;
        public async Task OnUpdate(Update update)
        {
            if (update is { CallbackQuery: { } query })
            {
                await _botClient.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
                await _botClient.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
            }

            if (update is { Type: UpdateType.ChatMember, ChatMember.NewChatMember.User.Id: var userId } && userId == _botClient.BotId)
            {
                var addedByUser = update.ChatMember.From.Id;

                if (await _userRepo.UserIdExistsAsync(addedByUser))
                {
                    await _botClient.LeaveChat(update.ChatMember.Chat.Id);
                    Console.WriteLine($"Бот покинул группу, добавивший пользователь: {update.ChatMember.From.Username}");
                }
                else
                {
                    Console.WriteLine($"Бот добавлен в группу авторизованным пользователем: {update.ChatMember.From.Username}");
                }
            }
        }
    }
}
