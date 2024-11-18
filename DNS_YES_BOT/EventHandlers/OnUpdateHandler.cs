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
            if (update is { CallbackQuery: { } query } && query.Data is { } message && message.Contains("add_admin")) 
            {
                await _botClient.AnswerCallbackQuery(query.Id, $"Вы выбрали {query.Data}");
                var parts = message.Split('_');
                long id = long.Parse(parts[2]);
                
                if (! await _userRepo.UserIdExistsAsync(id))
                {
                    await _userRepo.AddUserIdAsync(id);
                    await _botClient.SendMessage(query.Message!.Chat, $"Пользователь {query.From} добавил нового администратора {query.Data}");
                } else
                {
                    await _botClient.SendMessage(query.Message!.Chat, $"Пользователь {query.From} пытается добавить администратора {query.Data}, но он уже существует!");
                }
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
