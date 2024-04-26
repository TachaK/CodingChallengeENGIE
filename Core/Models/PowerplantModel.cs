
namespace Core.Models
{
    public class PowerplantModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PMin { get; set; }
        public decimal PMax { get; set; }
        public decimal Merit { get; set; }
        public decimal Cost { get; set; }
    }
}
