namespace DNS_YES_BOT.Models
{
    public class Shop
    {
        public Guid ShopId;
        public string ShopName = string.Empty;
        public List<Guid> EmployeesId = [];
    }
}
