namespace DNS_YES_BOT.Models
{
    public class VoteEntity
    {
        public DateTime? Date { get; set; } = DateTime.Now;
        public Dictionary<Guid, List<string>> VoteResults { get; set; } = [];
    }
}
