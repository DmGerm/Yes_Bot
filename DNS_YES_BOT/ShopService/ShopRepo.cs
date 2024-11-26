using DNS_YES_BOT.Models;

namespace DNS_YES_BOT.ShopService
{
    public class ShopRepo : IShopRepo
    {
        private readonly List<Shop> _shops = [];
        public Task AddShopAsync(string shopName)
        {
            _shops.Add(new() { ShopName = shopName, ShopId = Guid.NewGuid() });
            return Task.CompletedTask;
        }
        public Task<List<Shop>> GetShopsAsync() => Task.FromResult(_shops);
        

        public Task<List<string>> GetShopNamesAsync() => Task.FromResult(_shops.Select(x => x.ShopName).ToList());

        public Task<bool> IsShopExistAsync(string shopName) => Task.FromResult(_shops.Any(x => x.ShopName == shopName));

        public Task RemoveShopAsync(string shopName)
        {
            _shops.Remove(_shops.FirstOrDefault(x => x.ShopName == shopName) ??
                          throw new InvalidOperationException("Shop not found"));
            return Task.CompletedTask;
        }
    }
}
