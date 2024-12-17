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

        public Task<VoteEntity> GetVoteEntityByChatId(long chatId) => Task.FromResult(_votes[chatId]);

        public Task<bool> RemoveEntity(long chatId) => Task.FromResult(_votes.Remove(chatId));

        private void SaveVotesResult()
        {
            var json = JsonSerializer.Serialize(_votes);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "votes.json"), json);
        }
        private void LoadVotesDict()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "votes.json");

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
