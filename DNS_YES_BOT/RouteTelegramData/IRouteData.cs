using DNS_YES_BOT.Models;

namespace DNS_YES_BOT.RouteTelegramData
{
    public interface IRouteData : IDisposable
    {
        public Task SendDataAsync();
        public Task<string> GetVoteUrlAsync(VoteEntity voteEntity);
    }
}
