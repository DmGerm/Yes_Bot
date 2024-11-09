using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using var cts = new CancellationTokenSource();

var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
IConfiguration configuration = builder.Build();
var botToken = configuration["Telegram:BotToken"] ?? "";


var bot = new TelegramBotClient(botToken, cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); 

async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception);
    await Task.CompletedTask;
}


async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        await bot.SendMessage(msg.Chat, "Welcome! Pick one direction",
            replyMarkup: new InlineKeyboardMarkup().AddButtons("Left", "Right"));
    }
}


async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query }) 
    {
        await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
        await bot.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
    }
}