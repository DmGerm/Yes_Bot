using Interface.Models;

namespace Interface.VoteStorage
{
    public interface IVoteService
    {
        public string GetPageUrl(VoteEntity voteEntity);
        public List<string> GetShopsNames();
        public VoteEntity GetVoteResult(string token);
        public bool PostVoteByTokenAsync(VoteEntity vote);
        public void SyncShops(List<string> shops);
    }
}
