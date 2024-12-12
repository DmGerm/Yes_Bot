namespace DNS_YES_BOT.Models
{
    public class VoteEntity
    {
        public DateTime? Date { get; set; } = DateTime.Now;
        public Dictionary<Guid, List<string>> VoteResults { get; set; } = [];
        public void AddResult(Guid guid, string value)
        {
            if (!VoteResults.ContainsKey(guid))
            {
                VoteResults.Add(guid, [value]);
            }
            else
            {
                VoteResults[guid].Add(value);
            }
        }
    }
}
