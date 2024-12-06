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
                VoteResults = new Dictionary<Guid, string> { { shopId, userName } }
            });
            return Task.FromResult(true);
        }

        public Task<VoteEntity> GetVoteByShopId()
        {
            throw new NotImplementedException();
        }

        public Task<VoteEntity> GetVoteEntityByChatId(long chatId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveEntity(long chatId)
        {
            throw new NotImplementedException();
        }
    }
}
