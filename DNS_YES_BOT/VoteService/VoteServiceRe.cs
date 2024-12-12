using DNS_YES_BOT.Models;
using System.Text.Json;

namespace DNS_YES_BOT.VoteService
{
    public class VoteServiceRe : IVoteService
        //Todo: реализовать функцию сохранения
    {
        private Dictionary<long, VoteEntity> _votes = [];
         public Task<bool> AddEntity(long chatId, Guid shopId, string userName)
        {
            if (_votes.ContainsKey(chatId))
            {
                _votes[chatId].AddResult(shopId, userName);
            }
            else
            {
                _votes.Add(chatId, new VoteEntity()
                {
                    VoteResults = []
                });
            }
            SaveVotesResult();
            return Task.FromResult(true);
        }

        public Task<bool> CheckEntity(long chatId) => Task.FromResult(_votes.ContainsKey(chatId));

        public Task<VoteEntity> GetVoteByShopId()
        {
            throw new NotImplementedException();
        }

        public Task<VoteEntity> GetVoteEntityByChatId(long chatId) => Task.FromResult(_votes[chatId]);

        public Task<bool> RemoveEntity(long chatId) => Task.FromResult(_votes.Remove(chatId));

        private void SaveVotesResult()
        {
            var json = JsonSerializer.Serialize(_votes);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "votes.json"), json);
        }
    }
}
