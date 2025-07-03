using Newtonsoft.Json;

namespace FruitsBackend.Models
{
    public class Fruit
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string Order { get; set; } = string.Empty;
        public string Genus { get; set; } = string.Empty;

        public Nutritions? Nutritions { get; set; }
    }
}