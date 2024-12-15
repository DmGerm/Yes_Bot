using DNS_YES_BOT.ShopService;
using DNS_YES_BOT.VoteService;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnMessageHandler(TelegramBotClient telegramBotClient, IShopRepo shopRepo, IVoteService voteService)
    {
        private readonly TelegramBotClient _botClient = telegramBotClient;
        private readonly IShopRepo _shopRepo = shopRepo;
        private readonly IVoteService _voteService = voteService;
        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.From is null || msg.Text is null)
            {
                return;
            }
            var command = msg.Text.Split('@')[0];

            if (command == "/start")
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
                        $"vote_{shop.ShopId}"))
                          .ToList();
                await _botClient.SendMessage(
              msg.Chat.Id,
              "Нажмите кнопку вашего магазина, чтобы подтвердить ознакомление с информацией:",
              replyMarkup: new InlineKeyboardMarkup(buttons));
            }

            if (command == "/showResults")
            {
                var results = await _voteService.GetResultsAsync(msg.Chat.Id);
                var resultMessage = string.Join("\n\n",
                                           results.VoteResults.Select(item =>
                                           $"{_shopRepo.GetShopNameById(item.Key)}: {string.Join(", ", item.Value)}"));
            
                await _botClient.SendMessage(msg.From.Id, resultMessage);
            }

            if (command == "/admin_service")
            {
                await _botClient.SendMessage(
               msg.Chat.Id,
               "Выберите действие:",
               replyMarkup: new InlineKeyboardMarkup(
               [
                [
                    InlineKeyboardButton.WithCallbackData("Добавить администратора", "admin_add"),
                    InlineKeyboardButton.WithCallbackData("Добавить сотрудника", "employee_add")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("Добавить магазин", "shop_add"),
                    InlineKeyboardButton.WithCallbackData("Вывести список магазинов", "shops_show"),
                    InlineKeyboardButton.WithCallbackData("Удалить магазин", "shop_del")
                ]
               ]));

            }
        }
    }
}
