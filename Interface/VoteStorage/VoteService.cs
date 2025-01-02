using Interface.Models;
using System.Collections.Concurrent;

namespace Interface.VoteStorage
{
    public class VoteService : IVoteService
    {
        private List<string> _shops = [];
        private readonly ConcurrentDictionary<string, VoteEntity> _voteSessions = new();
        private readonly TimeSpan _linkExpiryTime = TimeSpan.FromMinutes(30);

        public string GetPageUrl(VoteEntity voteEntity)
        {
            if (voteEntity == null)
                throw new ArgumentNullException(nameof(voteEntity));

            string token = Guid.NewGuid().ToString();

            _voteSessions[token] = voteEntity;

            _ = Task.Run(async () =>
            {
                await Task.Delay(_linkExpiryTime);
                _voteSessions.TryRemove(token, out _);
            });

            return $"/vote/{token}";
        }

        public List<string> GetShopsNames() => _shops;

        public VoteEntity GetVoteResult(string token)
        {
            _voteSessions.TryGetValue(token, out var voteEntity);
            return voteEntity ?? throw new KeyNotFoundException();
        }

        public void SyncShops(List<string> shops)
        {
            try
            {
                _shops.Clear();
                _shops.AddRange(shops);
            }
            catch
            {
                throw;
            }
        }
    }
}
