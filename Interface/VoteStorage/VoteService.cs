using Interface.Models;

namespace Interface.VoteStorage
{
    public class VoteService : IVoteService
    {
        private Dictionary<long, VoteEntity> _votesStorage = new();

        public Dictionary<long, VoteEntity> GetVotes() => _votesStorage;

        public void SyncVotes(Dictionary<long, VoteEntity> votes)
        {
            _votesStorage.Clear();
            _votesStorage = votes;
        }
    }
}
