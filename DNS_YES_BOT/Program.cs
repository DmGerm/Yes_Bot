using DNS_YES_BOT.BotService;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfiguration configuration = builder.Build();

        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
                      ?? configuration["Telegram:BotToken"]
                      ?? throw new InvalidOperationException("Bot token is not provided!");

        BotService botService = new(botToken);
        await botService.BotRun();
    }
}