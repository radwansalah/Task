using FruitsBackend.Helpers;
using FruitsBackend.Models;

namespace FruitsBackend.Services
{
    public interface IFruitsService
    {
        Task<FruitResponse> GetFruitesByMinAndMaxSugar(int minSugar, int maxSugar);
    }
}