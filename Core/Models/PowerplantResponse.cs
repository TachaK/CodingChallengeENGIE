
namespace Core.Models
{
    public class PowerplantResponse
    {
        public PowerplantResponse(string name, decimal power)
        {
            Name = name;
            P = power;
        }

        public string Name { get; set; }
        public decimal P { get; set; }
    }
}
