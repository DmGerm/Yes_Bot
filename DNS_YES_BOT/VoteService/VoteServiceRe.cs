using DNS_YES_BOT.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNS_YES_BOT.VoteService
{
    public class VoteServiceRe : IVoteService
    {
        private Dictionary<long, VoteEntity> _votes = [];

        public VoteServiceRe()
        {
            LoadVotesDict();
        }

        public Task<bool> AddEntity(long chatId, string shopName, string userName)
        {
            if (_votes.ContainsKey(chatId))
            {
                _votes[chatId].AddResult(shopName, userName);
            }
            else
            {
                if (_votes.Count >= 50)
                {
                    var oldestKey = _votes.Keys.First();
                    _votes.Remove(oldestKey);
                }
                _votes.Add(chatId, new VoteEntity()
                {
                    VoteResults = new Dictionary<string, List<string>>
                        {
                             { shopName, new List<string> { userName } }
                        }
                });
            }
            SaveVotesResult();
            return Task.FromResult(true);
        }

        public Task<bool> CheckEntity(long chatId) => Task.FromResult(_votes.ContainsKey(chatId));

        public Task<VoteEntity> GetResultsAsync(long id) => Task.FromResult(_votes[id]);

        public Task<VoteEntity?> GetVoteEntityByChatId(long chatId)
        {
            if (_votes.TryGetValue(chatId, out var result))
            {
                return Task.FromResult<VoteEntity?>(result);
            }

            Console.WriteLine($"Vote with chat ID {chatId} not found.");
            return Task.FromResult<VoteEntity?>(null);
        }

        public Task<bool> RemoveEntity(long chatId) => Task.FromResult(_votes.Remove(chatId));

        private void SaveVotesResult()
        {
            var json = JsonSerializer.Serialize(_votes);
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var filePath = Path.Combine(directory, "votes.json");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, json);
        }
        private void LoadVotesDict()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data/votes.json");

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };

                _votes.Clear();
                _votes = JsonSerializer.Deserialize<Dictionary<long, VoteEntity>>(json, options) 
                                          ?? throw new InvalidOperationException("Invalid json");
            }
        }
    }
}
