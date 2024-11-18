using DNS_YES_BOT.BotService;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
IConfiguration configuration = builder.Build();
var botToken = configuration["Telegram:BotToken"] ?? "";

BotService botService = new(botToken);
await botService.BotRun();

