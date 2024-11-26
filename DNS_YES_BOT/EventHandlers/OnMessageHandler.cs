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
            if (msg.From is null || msg.Text is null)
            {
                return;
            }
            var command = msg.Text.Split('@')[0];

            if (msg.Text == "/start")
            {
                await _botClient.SendMessage(msg.Chat, "Выберите ваш филиал, чтобы подтвердить прочтение информации!",
                    replyMarkup: new InlineKeyboardMarkup().AddButtons("Выборг Советская", "Тихвин"));
            }

            if (msg.Text == "/admin_service")
            {
                await _botClient.SendMessage(msg.Chat.Id, "hi");
                await _botClient.SendMessage(msg.Chat.Id,
                    "Выберите действие:",
                    replyMarkup: new InlineKeyboardMarkup(
                    [
                    [InlineKeyboardButton.WithCallbackData("Добавить администратора", "admin_add")],
                    [InlineKeyboardButton.WithCallbackData("Добавить магазин", "shop_add")],
                    [InlineKeyboardButton.WithCallbackData("Добавить сотрудника", "employee_add")]
                    ]));

            }
        }
    }
}
