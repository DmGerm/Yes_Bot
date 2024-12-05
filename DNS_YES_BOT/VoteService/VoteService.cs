using DNS_YES_BOT.Models;

namespace DNS_YES_BOT.VoteService
{
    internal class VoteService : IVoteService
    {
        public Task<bool> AddEntity(long chatId, Guid shopId, string userName)
        {
            throw new NotImplementedException();
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
