using DNS_YES_BOT.ShopService;
using DNS_YES_BOT.UserService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DNS_YES_BOT.VoteService
{
    public class VoteManager(IVoteService voteService, ITelegramBotClient botClient, IShopRepo shopRepo, IAdminRepo adminRepo)
    {
        private readonly IVoteService _voteService = voteService;
        private readonly ITelegramBotClient _botClient = botClient;
        private readonly Dictionary<long, CancellationTokenSource> _activeVotes = [];
        private readonly IShopRepo _shopRepo = shopRepo;
        private readonly IAdminRepo adminRepo = adminRepo;
        long chatId;

        public async Task StartVoteAsync(Message msg)
        {

            if (msg.Chat.Type is Telegram.Bot.Types.Enums.ChatType.Group)
            {
                chatId = msg.Chat.Id;
            }

            if (msg.Chat.Type is Telegram.Bot.Types.Enums.ChatType.Supergroup)
            {
                chatId = msg.MessageThreadId ?? msg.Chat.Id;
            }

            _botClient.DeleteMessage(msg.Chat.Id, msg.MessageId).Wait();

            if (msg.From is not null && !await adminRepo.UserIdExistsAsync(msg.From.Id))
            {
                await SendMessageToChannel(msg, "⚠️ Вы не являетесь администратором!");
                return;
            }

            if (_activeVotes.ContainsKey(chatId))
            {
                await SendMessageToChannel(msg, "Голосование уже запущено");
                return;
            }

            if (await _voteService.CheckEntity(chatId))
            {
                await SendMessageToChannel(msg, "⚠️ Голосование в этом чате уже было проведено.");
                return;
            }

            var shops = await _shopRepo.GetShopsAsync();
            if (!shops.Any())
            {
                await SendMessageToChannel(msg, "❌ Нет магазинов для голосования.");
                return;
            }

            var buttons = shops
                .Select(shop => InlineKeyboardButton.WithCallbackData(shop.ShopName, $"vote_{shop.ShopName}"))
                .Select((button, index) => new { button, index })
                .GroupBy(x => x.index / 2)
                .Select(group => group.Select(x => x.button).ToList())
                .ToList();

            var inlineKeyboard = new InlineKeyboardMarkup(buttons);
            await SendMessageToChannelWithReplyMarkup(msg, "🗳 Нажмите кнопку вашего магазина, чтобы подтвердить ознакомление с информацией. Голосование продлится 24 часа:", inlineKeyboard);
            var cts = new CancellationTokenSource();
            _activeVotes[chatId] = cts;
        }


        private async Task SendMessageToChannel(Message msg, string messageText)
        {
            if (msg.MessageThreadId != null)
            {
                await _botClient.SendMessage(
                    chatId: msg.Chat.Id,
                    replyParameters: msg.MessageId,
                    text: messageText
                    );
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: msg.Chat.Id,
                    text: messageText);
            }
        }

        private async Task SendMessageToChannelWithReplyMarkup(Message msg, string messageText, InlineKeyboardMarkup inlineKeyboard)
        {
            if (msg.MessageThreadId != null)
            {
                await _botClient.SendMessage(
                    chatId: msg.Chat.Id,
                    text: messageText,
                    replyParameters: msg.MessageId,
                    replyMarkup: inlineKeyboard);
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: msg.Chat.Id,
                    text: messageText, replyMarkup: inlineKeyboard);
            }
        }
    }
}
