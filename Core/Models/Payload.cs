using Core.Constants;

namespace Core.Models
{
    public class Payload
    {
        public decimal Load { get; set; }
        public Fuels Fuels { get; set; }
        public List<Powerplant> PowerPlants { get; set; }
    }

}