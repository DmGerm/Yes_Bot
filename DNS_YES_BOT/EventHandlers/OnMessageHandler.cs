using DNS_YES_BOT.ShopService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnMessageHandler(TelegramBotClient telegramBotClient, IShopRepo shopRepo)
    {
        private readonly TelegramBotClient _botClient = telegramBotClient;
        private readonly IShopRepo _shopRepo = shopRepo;
        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.From is null || msg.Text is null)
            {
                return;
            }
            var command = msg.Text.Split('@')[0];

            if (command == "/start")
            {
                var shops = await _shopRepo.GetShopsAsync();
                var buttons = shops
                    .Select(shop => InlineKeyboardButton.WithCallbackData(
                        shop.ShopName,
                        $"shop_{shop.ShopId}")) //Todo: Доработать обработку этого запроса
                          .ToList();
                await _botClient.SendMessage(
              msg.Chat.Id,
              "Нажмите кнопку вашего магазина, чтобы подтвердить ознакомление с информацией:",
              replyMarkup: new InlineKeyboardMarkup(buttons));
            }

            if (command == "/admin_service")
            {
                await _botClient.SendMessage(msg.Chat.Id, "hi");
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
