using Telegram.Bot;
using Telegram.Bot.Types;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnUpdateHandler(TelegramBotClient telegramBotClient)
    {
        private readonly TelegramBotClient botClient = telegramBotClient;
        public async Task OnUpdate(Update update)
        {
            if (update is { CallbackQuery: { } query })
            {
                await botClient.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
                await botClient.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
            }
        }
    }
}
