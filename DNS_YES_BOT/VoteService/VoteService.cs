using DNS_YES_BOT.Models;

namespace DNS_YES_BOT.VoteService
{
    public class VoteService : IVoteService
    {
        private Dictionary<long, VoteEntity> _votes = [];
         public Task<bool> AddEntity(long chatId, Guid shopId, string userName)
        {
            _votes.Add(chatId, new VoteEntity()
            {
                VoteResults = []
            });
            return Task.FromResult(true);
        }

        public Task<bool> CheckEntity(long chatId) => Task.FromResult(_votes.ContainsKey(chatId));

        public Task<VoteEntity> GetVoteByShopId()
        {
            throw new NotImplementedException();
        }

        public Task<VoteEntity> GetVoteEntityByChatId(long chatId) => Task.FromResult(_votes[chatId]);

        public Task<bool> RemoveEntity(long chatId) => Task.FromResult(_votes.Remove(chatId));
    }
}
