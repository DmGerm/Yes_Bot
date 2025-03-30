using DNS_YES_BOT.Models;

namespace DNS_YES_BOT.RouteTelegramData
{
    public interface IRouteData : IDisposable
    {
        public Task SendDataOnceAsync(List<string> shopList);
        public Task<string> GetVoteUrlAsync(VoteEntity voteEntity);
        public Task DataUpdateAsync(CancellationToken cancellationToken);
    }
}
