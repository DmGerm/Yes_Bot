using DNS_YES_BOT.Models;
using DNS_YES_BOT.VoteService;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DNS_YES_BOT.RouteTelegramData
{

    public class RouteData(CancellationToken cancellationToken, VoteServiceRe voteService) : IRouteData
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private bool disposedValue;
        private readonly VoteServiceRe? _voteService = voteService;
        /*        private string? _csrfToken;
        */
        public async Task SendDataOnceAsync(List<string> shopList)
        {
            try
            {
                if (shopList.Count > 0)
                {
                    /*                   if (string.IsNullOrEmpty(_csrfToken))
                                       {
                                           await GetCsrfTokenAsync();
                                       }*/

                    var json = JsonSerializer.Serialize(shopList);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    /*                    if (!string.IsNullOrEmpty(_csrfToken))
                                        {
                                            _httpClient.DefaultRequestHeaders.Remove("RequestVerificationToken");
                                            _httpClient.DefaultRequestHeaders.Add("RequestVerificationToken", _csrfToken);
                                        }*/

                    var response = await _httpClient.PostAsync("http://interface:7030/api/Vote/shop_sync", content, cancellationToken);

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
            }
        }

        public async Task<string> GetVoteUrlAsync(VoteEntity voteEntity)
        {
            if (voteEntity == null)
                throw new ArgumentNullException(nameof(voteEntity));

            try
            {
                HttpResponseMessage response = await PostVoteAsync(voteEntity);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get vote URL. Status code: {response.StatusCode}");
                }

                var link = await response.Content.ReadAsStringAsync();
                string pattern = @"[0-9a-fA-F\-]{36}";

                Match match = Regex.Match(link, pattern);

                if (match.Success)
                {
                    Guid token = Guid.Parse(match.Value);

                    if (voteEntity.EntityToken is null)
                        voteEntity.EntityToken = new List<Guid>();

                    voteEntity.EntityToken.Add(token);

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromHours(24));
                        lock (voteEntity.EntityToken)
                        {
                            voteEntity.EntityToken.RemoveAll(x => x == token);
                        }
                    });

                    return link;
                }

                throw new Exception("Failed to parse token from the response.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving vote URL: {ex.Message}", ex);
            }
        }

        private async Task<HttpResponseMessage> PostVoteAsync(VoteEntity voteEntity)
        {
            var json = JsonSerializer.Serialize(voteEntity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://interface:7030/api/vote/vote_link", content);
            return response;
        }

        /*        public async Task<string> GetCsrfTokenAsync()
       {
           try
           {
               var response = await _httpClient.GetAsync("http://interface:7030/api/vote/csrf-token", cancellationToken);

               if (response.IsSuccessStatusCode)
               {
                   var responseBody = await response.Content.ReadAsStringAsync();
                   var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
                   _csrfToken = tokenResponse?["token"];
                   return _csrfToken ?? throw new Exception("Token not found in response.");
               }

               throw new Exception($"Failed to retrieve CSRF token. Status code: {response.StatusCode}");
           }
           catch (Exception ex)
           {
               throw new Exception($"Error retrieving CSRF token: {ex.Message}");
           }
       }*/
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

        public Task DataUpdateAsync(CancellationToken cancellationToken)
        {
            return DataUpdateAsync(_voteService, cancellationToken);
        }

        public async Task DataUpdateAsync(VoteServiceRe? _voteService, CancellationToken cancellationToken)
        {
            if (_voteService == null)
            {
                throw new ArgumentNullException(nameof(_voteService));
            }

            _voteService._votes.Values
                .ToList()
                .ForEach(voteEntity => voteEntity.EntityToken = new List<Guid>());

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                if (_voteService._votes.Count != 0)
                {
                    var votes = _voteService._votes.Values.ToList();

                    var jsonList = votes
                        .Where(voteEntity => voteEntity.EntityToken != null && voteEntity.EntityToken.Count != 0)
                        .Select(voteEntity => JsonSerializer.Serialize(voteEntity))
                        .ToList();

                    var contentList = jsonList
                        .Select(json => new StringContent(json, Encoding.UTF8, "application/json"))
                        .ToList();

                    var tasks = contentList.Select(async content =>
                    {
                        try
                        {
                            var response = await _httpClient.PostAsync("http://interface:7030/api/vote/vote_by_token", content, cancellationToken);

                            if (!response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Error posting vote data. Status code: {response.StatusCode}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception while sending request: {ex.Message}");
                        }
                        finally
                        {
                            content.Dispose();
                        }
                    });

                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Operation cancelled.");
                    }
                }
            }
        }
    }
}
