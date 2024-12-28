using Interface.Models;

namespace Interface.VoteStorage
{
    public interface IVoteService
    {
        public void SyncVotes(Dictionary<long, VoteEntity> votes);
        public Dictionary<long, VoteEntity> GetVotes();
    }
}
