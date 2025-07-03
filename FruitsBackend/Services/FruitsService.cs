using System.Net;
using System.Text.Json;
using FruitsBackend.Helpers;
using FruitsBackend.Models;

namespace FruitsBackend.Services
{
    public class FruitsService : IFruitsService
    {
        private readonly HttpClient _httpClient;
        public FruitsService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("FruityClient");
        }

        public async Task<FruitResponse> GetFruitesByMinAndMaxSugar(int minSugar, int maxSugar)
        {
            HashSet<Fruit>? fruits = new HashSet<Fruit>();
            try
            {
                // I didn't add max as the fruityvice has a default value for max which is 1000
                var response = await _httpClient.GetAsync(@"fruit/sugar?min=0");
                if (response == null || !response.IsSuccessStatusCode)
                {
                    return new FruitResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadGateway
                    };
                }

                string json = await response.Content.ReadAsStringAsync();
                if (json == null)
                {
                    return new FruitResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadGateway
                    };
                }

                fruits = JsonSerializer.Deserialize<HashSet<Fruit>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (fruits == null || !fruits.Any())
                {
                    return new FruitResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception)
            {
                return new FruitResponse
                {
                    StatusCode = (int)HttpStatusCode.BadGateway
                };
            }

            return new FruitResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Fruits = fruits.Where(fruit => fruit.Nutritions != null &&
                                               fruit.Nutritions.Sugar >= minSugar &&
                                               fruit.Nutritions.Sugar <= maxSugar)
                               .OrderByDescending(fruit => fruit.Nutritions?.HealthScore)
                               .ToList(),
            };
        }
    }
}