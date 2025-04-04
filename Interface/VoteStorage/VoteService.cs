using Interface.Models;
using Microsoft.AspNetCore.Hosting.Server;
using System.Collections.Concurrent;

namespace Interface.VoteStorage
{
    public class VoteService(IServer server) : IVoteService
    {
        private List<string> _shops = [];
        private readonly ConcurrentDictionary<string, VoteEntity> _voteSessions = new();
        private readonly TimeSpan _linkExpiryTime = TimeSpan.FromHours(24);
        private readonly IServer _server = server;

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

            var serverAddress = GetServerAddress();
            return $"{serverAddress}/vote/{token}";
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

        public bool PostVoteByTokenAsync(VoteEntity vote)
        {
            if (vote.EntityToken == null)
                throw new ArgumentNullException(nameof(vote.EntityToken));

            bool result = true;
            foreach (var token in vote.EntityToken)
            {
                if (!_voteSessions.TryGetValue(token.ToString(), out var voteEntity))
                {
                    result = false;
                }
                else
                {
                    _voteSessions[token.ToString()] = vote;
                }
            }
            return result;
        }

        private string GetServerAddress()
        {
            // Получаем домен из переменной окружения или конфигурации
            var domain = Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("PORT_NUMBER") ?? "8080";

            return $"http://{domain}:{port}";
        }

    }
}
