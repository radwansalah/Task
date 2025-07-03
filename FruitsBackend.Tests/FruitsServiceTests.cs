namespace FruitsBackend.Tests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Moq;
    using Moq.Protected;
    using System.Text.Json;
    using FruitsBackend.Models;
    using FruitsBackend.Services;

    public class FruitsServiceTests
    {
        private FruitsService CreateServiceWithResponse(HttpStatusCode statusCode, string content)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://www.fruityvice.com/api/")
            };

            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("FruityClient")).Returns(httpClient);

            return new FruitsService(factoryMock.Object);
        }

        [Fact]
        public async Task GetFruitesByMinAndMaxSugar_ReturnsBadGateway_OnHttpFailure()
        {
            var service = CreateServiceWithResponse(HttpStatusCode.InternalServerError, "");

            var result = await service.GetFruitesByMinAndMaxSugar(0, 100);

            Assert.Equal((int)HttpStatusCode.BadGateway, result.StatusCode);
        }

        [Fact]
        public async Task GetFruitesByMinAndMaxSugar_ReturnsNotFound_WhenEmptyData()
        {
            var service = CreateServiceWithResponse(HttpStatusCode.OK, "[]");

            var result = await service.GetFruitesByMinAndMaxSugar(0, 100);

            Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task GetFruitesByMinAndMaxSugar_ReturnsFilteredFruits()
        {
            var fruits = new[]
            {
                new Fruit
                {
                    Name = "Apple",
                    Nutritions = new Nutritions { Sugar = 15, Protein = 1, Carbohydrates = 10, Fat = 1 }
                },
                new Fruit
                {
                    Name = "Banana",
                    Nutritions = new Nutritions { Sugar = 5, Protein = 1, Carbohydrates = 20, Fat = 0.2 }
                }
            };

            string json = JsonSerializer.Serialize(fruits);
            var service = CreateServiceWithResponse(HttpStatusCode.OK, json);

            var result = await service.GetFruitesByMinAndMaxSugar(0, 10);

            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.Single(result.Fruits);
            Assert.Equal("Banana", result.Fruits[0].Name);
        }

        [Fact]
        public async Task GetFruitesByMinAndMaxSugar_ReturnsFilteredFruitsSortedByHealthiest()
        {
            var fruits = new[]
            {
                new Fruit
                {
                    Name = "Apple",
                    Nutritions = new Nutritions { Sugar = 15, Protein = 1, Carbohydrates = 10, Fat = 1 }
                },
                new Fruit
                {
                    Name = "Banana",
                    Nutritions = new Nutritions { Sugar = 5, Protein = 1, Carbohydrates = 20, Fat = 0.2 }
                },
                new Fruit
                {
                    Name = "Tomato",
                    Nutritions = new Nutritions { Sugar = 3, Protein = 0.9, Carbohydrates = 4, Fat = 0.2 }
                }
            };

            string json = JsonSerializer.Serialize(fruits);
            var service = CreateServiceWithResponse(HttpStatusCode.OK, json);

            var result = await service.GetFruitesByMinAndMaxSugar(0, 10);

            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(2, result.Fruits.Count);
            Assert.Equal("Banana", result.Fruits[0].Name);
            Assert.Equal("Tomato", result.Fruits[1].Name);
            Assert.NotNull(result.Fruits[0].Nutritions);
            Assert.NotNull(result.Fruits[1].Nutritions);
            Assert.Equal(6.6, result.Fruits[0].Nutritions.HealthScore, 1);
            Assert.Equal(0.4, result.Fruits[1].Nutritions.HealthScore, 1);
        }
    }
}

