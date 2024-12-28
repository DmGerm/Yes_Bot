using Interface.Models;

namespace Interface.VoteStorage
{
    public interface IVoteService
    {
        public string GetPageUrl(VoteEntity voteEntity);
        public List<string> GetShopsNames();
        public List<string> GetUsersNamesByShop(string shopName);
        public List<string> GetVotedShops();
        public List<string> GenNotVotedShops();
        public void SyncShops(List<string> shops);
    }
}
