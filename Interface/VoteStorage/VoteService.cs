using Interface.Models;

namespace Interface.VoteStorage
{
    public class VoteService : IVoteService
    {
        public List<string> GenNotVotedShops()
        {
            throw new NotImplementedException();
        }

        public string GetPageUrl(VoteEntity voteEntity)
        {
            throw new NotImplementedException();
        }

        public List<string> GetShopsNames()
        {
            throw new NotImplementedException();
        }

        public List<string> GetUsersNamesByShop(string shopName)
        {
            throw new NotImplementedException();
        }

        public List<string> GetVotedShops()
        {
            throw new NotImplementedException();
        }

        public void SyncShops(List<string> shops)
        {
            throw new NotImplementedException();
        }

        private async Task RemoveVoteUrlByTimer()
        {
            throw new NotImplementedException();
        }
    }
}
