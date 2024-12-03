using DNS_YES_BOT.Models;

namespace DNS_YES_BOT.ShopService
{
    public interface IShopRepo
    {
        public Task AddShopAsync(string shopName);
        public Task<Guid> GetShopIdAsync(string shopName);
        public Task<List<Shop>> GetShopsAsync();
        public Task<List<string>> GetShopNamesAsync();
        public Task<bool> IsShopExistAsync(string shopName);
        public Task RemoveShopAsync(string shopName);
        public Task AddEmploeyeeToShopAsync(Guid shopId, Employee employeeId);
        public Task RemoveEmployeeFromShopAsync(Guid shopId, Guid empoyeeId);
    }
}
