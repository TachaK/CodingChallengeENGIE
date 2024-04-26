
namespace Core.Models
{
    public class PowerplantResponse
    {
        public PowerplantResponse(string name, decimal power)
        {
            Name = name;
            Power = power;
        }

        public string Name { get; set; }
        public decimal Power { get; set; }
    }
}
