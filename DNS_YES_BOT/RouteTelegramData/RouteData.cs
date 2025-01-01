using DNS_YES_BOT.ShopService;
using System.Text;
using System.Text.Json;

namespace DNS_YES_BOT.RouteTelegramData
{
    public class RouteData(IShopRepo shopRepo, CancellationToken cancellationToken) : IRouteData, IDisposable
    {
        private readonly IShopRepo _shopRepo = shopRepo;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool disposedValue;

        public async Task SendDataAsync()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    List<string> shopList = await _shopRepo.GetShopNamesAsync();
                    if (shopList.Count > 0)
                    {
                        var json = JsonSerializer.Serialize(shopList);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PostAsync("https://localhost:7030/api/Vote/shop_sync", content, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Data sent successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"Error sending data: {response.StatusCode}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No shops found to sync.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    await Task.Delay(30000, cancellationToken); 
                    continue;
                }

                await Task.Delay(30000, cancellationToken);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                    _httpClient.Dispose();
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~RouteData()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
