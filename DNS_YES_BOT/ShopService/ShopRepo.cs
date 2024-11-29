using DNS_YES_BOT.Models;
using System.Text.Json;

namespace DNS_YES_BOT.ShopService
{
    public class ShopRepo : IShopRepo
    {
        private readonly List<Shop> _shops = [];
        public ShopRepo()
        {
            LoadShopList();
        }

        public Task AddShopAsync(string shopName)
        {
            _shops.Add(new() { ShopName = shopName, ShopId = Guid.NewGuid() });
            SaveShopsList();
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

        private void LoadShopList()
        {
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shops.json")))
            {
                var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userIds.json"));
                _shops.Clear();
                _shops.AddRange(JsonSerializer.Deserialize<List<Shop>>(json) ?? throw new InvalidOperationException("Invalid json"));
            }
        }

        private void SaveShopsList()
        {
            var json = JsonSerializer.Serialize(_shops);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shops.json"), json);
        }
    }
}
