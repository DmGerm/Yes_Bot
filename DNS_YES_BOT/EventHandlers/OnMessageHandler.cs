using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnMessageHandler(TelegramBotClient telegramBotClient)
    {
        private readonly TelegramBotClient _botClient = telegramBotClient;
        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.Text == "/start")
            {
                await _botClient.SendMessage(msg.Chat, "Выберите ваш филиал, чтобы подтвердить прочтение информации!",
                    replyMarkup: new InlineKeyboardMarkup().AddButtons("Выборг Советская", "Тихвин"));
            }
        }
    }
}
