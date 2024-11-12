using Telegram.Bot.Polling;

namespace DNS_YES_BOT.EventHandlers
{
    public class OnErrorHandler
    {
        public static async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
            await Task.CompletedTask;
        }
    }
}
