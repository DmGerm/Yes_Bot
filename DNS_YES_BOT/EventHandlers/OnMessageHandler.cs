using DNS_YES_BOT.UserService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnMessageHandler(TelegramBotClient telegramBotClient, IUserRepo userRepo)
    {
        private readonly TelegramBotClient _botClient = telegramBotClient;
        private readonly IUserRepo _userRepo = userRepo;
        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.From == null)
            {
                return;
            }

            if (msg.Text == "/start")
            {
                await _botClient.SendMessage(msg.Chat, "Выберите ваш филиал, чтобы подтвердить прочтение информации!",
                    replyMarkup: new InlineKeyboardMarkup().AddButtons("Выборг Советская", "Тихвин"));
            }

            if (msg.Text == "/add_admin")
            {
                if (await _userRepo.UserListIsEmptyAsync())
                {
                    await _userRepo.AddAdminAsync(msg.From.Id);
                    await _botClient.SendMessage(msg.Chat, "Вы стали первым администратором чата.");
                    return;
                }

                if (!await _userRepo.UserIdExistsAsync(msg.From.Id))
                {
                    await _botClient.SendMessage(msg.Chat, "Вы не являетесь администратором!");
                    return;
                }

                var administrators = await _botClient.GetChatAdministrators(msg.Chat.Id);

                if (administrators.Length != 0)
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(
                         administrators.Select(admin =>
                         {
                             var user = admin.User;
                             return new InlineKeyboardButton(user?.Username ?? "Unknown User")
                             {
                                 CallbackData = $"add_admin_{user?.Id}"
                             };
                         }).ToArray());

                    await _botClient.SendMessage(msg.Chat, "Выберите пользователя для добавления в список администраторов:",
                        replyMarkup: inlineKeyboard);
                }
                else
                {
                    await _botClient.SendMessage(msg.Chat, "В чате нет администраторов.");
                }

            }
        }
    }
}
