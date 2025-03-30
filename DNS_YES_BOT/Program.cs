using DNS_YES_BOT.BotService;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfiguration configuration = builder.Build();

        var stringFromEnv = File.ReadAllText("token.env");
        string pattern = @"^(?:\w+)=([\w:]+)$";
        if (!Regex.IsMatch(stringFromEnv, pattern))
        {
            throw new InvalidOperationException("Invalid token.env file format.");
        }
        var token = Regex.Match(stringFromEnv, pattern).Groups[1].Value;
        Console.WriteLine(token);
        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
                                        ?? token
                                        ?? throw new InvalidOperationException("Bot token is not provided!");

        BotService botService = new(botToken);
        await botService.BotRun();
    }
}