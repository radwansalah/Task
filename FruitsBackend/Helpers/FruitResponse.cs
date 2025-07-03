using FruitsBackend.Models;

namespace FruitsBackend.Helpers
{
    public class FruitResponse
    {
        public int StatusCode { get; set; }
        public List<Fruit> Fruits { get; set; } = new List<Fruit>();
    }

}