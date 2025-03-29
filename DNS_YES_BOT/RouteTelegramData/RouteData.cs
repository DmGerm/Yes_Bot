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
                /*                if (string.IsNullOrEmpty(_csrfToken))
                                {
                                    await GetCsrfTokenAsync();
                                }*/

                HttpResponseMessage response = await PostVoteAsync(voteEntity);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get vote URL. Status code: {response.StatusCode}");
                }

                var link = await response.Content.ReadAsStringAsync();
                string pattern = @"[^/] +(?=/$|$)";

                Match match = Regex.Match(link, pattern);
                Guid token = Guid.Parse(match.Value);

                if (voteEntity.EntityToken is null)
                    voteEntity.EntityToken = [];

                if (match.Success)
                    voteEntity.EntityToken.Add(token);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(30));
                    voteEntity.EntityToken.RemoveAll(x => x == token);
                });

                return link;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving vote URL: {ex.Message}");
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

        public Task DataUpdateAsync()
        {
            while (cancellationToken.IsCancellationRequested)
            {
                if (_voteService != null && _voteService._votes.Count != 0)
                {
                    Task.Run(() =>
                    {
                        var votes = _voteService._votes.Values.ToList();
                        var jsonList = votes.Select(voteEntity => JsonSerializer.Serialize(voteEntity));
                        var content = jsonList.Select(json => new StringContent(json, Encoding.UTF8, "application/json"));


                        content.Select(async content =>
                        {
                            await _httpClient.PostAsync("http://interface:7030/api/vote/vote_by_token", content);
                        });
                    });
                }
            }
            return Task.CompletedTask;
        }
    }
}
