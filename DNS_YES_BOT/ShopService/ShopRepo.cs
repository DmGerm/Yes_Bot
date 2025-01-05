using DNS_YES_BOT.Models;
using DNS_YES_BOT.RouteTelegramData;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNS_YES_BOT.ShopService
{
    public class ShopRepo() : IShopRepo
    {
        private readonly HttpClient _httpClient = new();
        private readonly List<Shop> _shops = [];
        private readonly IRouteData? _routeData;
        public ShopRepo(IRouteData routeData) : this()
        {
            _routeData = routeData;
            try
            {
                LoadShopList();
                if (_shops.Count != 0)
                    SendShopToController(_shops.Select(x => x.ShopName).ToList()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task AddShopAsync(string shopName)
        {
            _shops.Add(new() { ShopName = shopName, ShopId = Guid.NewGuid() });

            try
            {
                SaveShopsList();
                await _routeData.SendDataOnceAsync(await GetShopNamesAsync());
                if (_shops.Count != 0)
                    SendShopToController(_shops.Select(x => x.ShopName).ToList()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public Task<List<Shop>> GetShopsAsync() => Task.FromResult(_shops);


        public Task<List<string>> GetShopNamesAsync() => Task.FromResult(_shops.Select(x => x.ShopName).ToList());

        public Task<bool> IsShopExistAsync(string shopName) => Task.FromResult(_shops.Any(x => x.ShopName == shopName));

        public async Task RemoveShopAsync(string shopName)
        {
            _shops.Remove(_shops.FirstOrDefault(x => x.ShopName == shopName) ??
                          throw new InvalidOperationException("Shop not found"));
            try
            {
                SaveShopsList();
                await _routeData.SendDataOnceAsync(await GetShopNamesAsync());
                if (_shops.Count != 0)
                    SendShopToController(_shops.Select(x => x.ShopName).ToList()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void LoadShopList()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data/shops.json");

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
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var filePath = Path.Combine(directory, "shops.json");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, json);
        }

        public Task<Guid> GetShopIdAsync(string shopName) => Task.FromResult(_shops.FirstOrDefault(x => x.ShopName == shopName)?.ShopId
                                                                            ?? throw new InvalidOperationException("Shop not found"));

        public object GetShopNameById(Guid key) => _shops.FirstOrDefault(x => x.ShopId == key)?.ShopName
                                                   ?? throw new InvalidOperationException("Shop not found");

        public Task<int> GetShopsCountAsync() => Task.FromResult(_shops.Count);

        private async Task SendShopToController(List<string> shops)
        {
            var json = JsonSerializer.Serialize(shops);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://localhost:7003/api/Vote/shop_sync", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
        }
    }
}
