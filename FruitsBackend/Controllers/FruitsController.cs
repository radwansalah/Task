using Microsoft.AspNetCore.Mvc;
using FruitsBackend.Services;

namespace FruitsBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FruitsController : ControllerBase
{
    private readonly ILogger<FruitsController> _logger;
    private readonly IFruitsService _fruitsService;

    public FruitsController(ILogger<FruitsController> logger, IFruitsService fruitsService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        _fruitsService = fruitsService ?? throw new ArgumentNullException(nameof(fruitsService), "FruitsService cannot be null.");
    }

    // GET: /Fruits/Healthiest?minSugar={minSugar}&maxSugar={maxSugar}
    [HttpGet("Healthiest")]
    public async Task<IActionResult> Healthiest(int? minSugar, int? maxSugar)
    {
        (bool flowControl, IActionResult value) = CheckParams(minSugar, maxSugar);
        if (!flowControl)
        {
            return value;
        }

        var fruitResponse = await _fruitsService.GetFruitesByMinAndMaxSugar(minSugar.Value, maxSugar.Value);   // minSugar and maxSugar are not null here for sure, we checked those param for null inside CheckParams method.
        (flowControl, value) = CheckFruitesStatusCode(minSugar, maxSugar, fruitResponse);
        if (!flowControl)
        {
            return value;
        }

        return Ok(new
        {
            data = fruitResponse.Fruits.Select(fruit => new
            {
                fruit.Name,
                Score = fruit.Nutritions != null ? Math.Round(fruit.Nutritions.HealthScore, 1) : 0,
            }),
        });
    }

    private (bool flowControl, IActionResult value) CheckFruitesStatusCode(int? minSugar, int? maxSugar, Helpers.FruitResponse fruitResponse)
    {
        if (fruitResponse.StatusCode == StatusCodes.Status404NotFound)
        {
            _logger.LogWarning("No fruits found with sugar between {MinSugar} and {MaxSugar}.", minSugar, maxSugar);
            return (flowControl: false, value: NotFound(new { error = "No fruits found with the specified sugar range." }));
        }
        if (fruitResponse.StatusCode == StatusCodes.Status502BadGateway)
        {
            _logger.LogError("Bad Gateway: The fruityvice server has a problem at the main time.");
            return (flowControl: false, value: StatusCode(502, new { error = "Bad Gateway: The fruityvice server has a problem at the main time" }));
        }
        if (fruitResponse.StatusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError("Internal Server Error: The fruityvice server has a problem at the main time.");
            return (flowControl: false, value: StatusCode(502, new { error = "Bad Gateway: The fruityvice server has a problem at the main time" }));
        }

        if (fruitResponse.StatusCode != StatusCodes.Status200OK)
        {
            _logger.LogError("Unexpected status code: {StatusCode} coming from fruityvice server API", fruitResponse.StatusCode);
            return (flowControl: false, value: StatusCode(fruitResponse.StatusCode, new { error = "An unexpected error occurred." }));
        }

        return (flowControl: true, value: Ok());
    }

    private (bool flowControl, IActionResult value) CheckParams(int? minSugar, int? maxSugar)
    {
        if (minSugar == null || maxSugar == null)
        {
            return (flowControl: false, value: BadRequest("Please provide valid min and max sugar values."));
        }

        if (minSugar < 0 || maxSugar < 0)
        {
            return (flowControl: false, value: BadRequest("Sugar values must be non-negative."));
        }

        if (minSugar > maxSugar)
        {
            return (flowControl: false, value: BadRequest("Minimum sugar cannot be greater than maximum sugar."));
        }

        return (flowControl: true, value: Ok());
    }
}