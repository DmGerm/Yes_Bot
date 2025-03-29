namespace DNS_YES_BOT.Models
{
    public class VoteEntity
    {
        public List<Guid>? EntityToken { get; set; } = [];
        public DateTime? Date { get; set; } = DateTime.Now;
        public Dictionary<string, List<string>> VoteResults { get; set; } = [];
        public void AddResult(string shopName, string value)
        {
            if (!VoteResults.ContainsKey(shopName))
            {
                VoteResults.Add(shopName, [value]);
            }
            else
            {
                VoteResults[shopName].Add(value);
            }
        }
    }
}
