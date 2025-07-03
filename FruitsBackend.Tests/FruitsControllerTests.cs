namespace FruitsBackend.Tests
{
    using Xunit;
    using Moq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using FruitsBackend.Controllers;
    using FruitsBackend.Models;
    using FruitsBackend.Services;
    using FruitsBackend.Helpers;

    public class FruitsControllerTests
    {
        private readonly FruitsController _controller;
        private readonly Mock<IFruitsService> _mockService;
        private readonly Mock<ILogger<FruitsController>> _mockLogger;

        public FruitsControllerTests()
        {
            _mockService = new Mock<IFruitsService>();
            _mockLogger = new Mock<ILogger<FruitsController>>();
            _controller = new FruitsController(_mockLogger.Object, _mockService.Object);
        }

        [Fact]
        public async Task Healthiest_ReturnsBadRequest_WhenMinOrMaxSugarIsNull()
        {
            var result1 = await _controller.Healthiest(null, 10);
            var result2 = await _controller.Healthiest(10, null);
            Assert.IsType<BadRequestObjectResult>(result1);
            Assert.IsType<BadRequestObjectResult>(result2);
        }

        [Fact]
        public async Task Healthiest_ReturnsBadRequest_WhenSugarValuesAreNegative()
        {
            var result = await _controller.Healthiest(-1, 10);
            Assert.IsType<BadRequestObjectResult>(result);

            result = await _controller.Healthiest(5, -5);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Healthiest_ReturnsBadRequest_WhenMinSugarGreaterThanMaxSugar()
        {
            var result = await _controller.Healthiest(20, 10);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Healthiest_ReturnsNotFound_WhenServiceReturns404()
        {
            _mockService.Setup(s => s.GetFruitesByMinAndMaxSugar(0, 100)).ReturnsAsync(new FruitResponse
            {
                StatusCode = StatusCodes.Status404NotFound
            });

            var result = await _controller.Healthiest(0, 100);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Healthiest_Returns502_WhenServiceReturnsBadGateway()
        {
            _mockService.Setup(s => s.GetFruitesByMinAndMaxSugar(0, 100)).ReturnsAsync(new FruitResponse
            {
                StatusCode = StatusCodes.Status502BadGateway
            });

            var result = await _controller.Healthiest(0, 100);
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(502, statusResult.StatusCode);
        }

        [Fact]
        public async Task Healthiest_Returns502_WhenServiceReturnsInternalServerError()
        {
            _mockService.Setup(s => s.GetFruitesByMinAndMaxSugar(0, 100)).ReturnsAsync(new FruitResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError
            });

            var result = await _controller.Healthiest(0, 100);
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(502, statusResult.StatusCode); // as mapped in controller
        }

        [Fact]
        public async Task Healthiest_ReturnsCustomError_WhenUnexpectedStatusCode()
        {
            _mockService.Setup(s => s.GetFruitesByMinAndMaxSugar(0, 100)).ReturnsAsync(new FruitResponse
            {
                StatusCode = StatusCodes.Status403Forbidden
            });

            var result = await _controller.Healthiest(0, 100);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        }

        [Fact]
        public async Task Healthiest_ReturnsOk_WithValidFruits()
        {
            var fruits = new List<Fruit>
            {
                new Fruit { Name = "Banana", Nutritions = new Nutritions { Protein = 1, Carbohydrates = 22, Sugar = 17.2, Fat = 0.2 } },
                new Fruit { Name = "Apple", Nutritions = new Nutritions { Protein = 0.3, Carbohydrates = 14, Sugar = 10, Fat = 0.2 } }
            };

            _mockService.Setup(s => s.GetFruitesByMinAndMaxSugar(0, 100)).ReturnsAsync(new FruitResponse
            {
                StatusCode = StatusCodes.Status200OK,
                Fruits = fruits
            });

            var result = await _controller.Healthiest(0, 100);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}

