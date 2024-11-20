namespace DNS_YES_BOT.Models
{
    internal class Shop
    {
        public Guid ShopId;
        public string ShopName = string.Empty;
        public List<Guid> EmployeesId = [];
    }
}
