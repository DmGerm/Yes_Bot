using DNS_YES_BOT.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            SaveShopsList();
            return Task.CompletedTask;
        }

        private void LoadShopList()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shops.json");

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };

                _shops.Clear();
                _shops.AddRange(JsonSerializer.Deserialize<List<Shop>>(json, options) ?? throw new InvalidOperationException("Invalid json"));
            }
        }

        private void SaveShopsList()
        {
            var json = JsonSerializer.Serialize(_shops);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shops.json"), json);
        }

        public Task<Guid> GetShopIdAsync(string shopName) => Task.FromResult(_shops.FirstOrDefault(x => x.ShopName == shopName)?.ShopId 
                                                                            ?? throw new InvalidOperationException("Shop not found"));

        public object GetShopNameById(Guid key) => _shops.FirstOrDefault(x => x.ShopId == key).ShopName 
                                                   ?? throw new InvalidOperationException("Shop not found");
    }
}
