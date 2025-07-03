namespace FruitsBackend.Models
{
    public class Nutritions
    {
        public int Calories { get; set; }

        public double Fat { get; set; }

        public double Sugar { get; set; }

        public double Carbohydrates { get; set; }

        public double Protein { get; set; }

        public double HealthScore => Protein * 2 + Carbohydrates * 0.5 - (Sugar + Fat * 2);
    }
}