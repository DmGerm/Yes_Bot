using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DNS_YES_BOT.Models
{
    public class Shop
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public List<Guid> EmployeesId { get; set; } = [];
    }
}
