
namespace Core.Models
{
    public class Powerplant
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PMin { get; set; }
        public decimal PMax { get; set; }

        public Powerplant(string name, string type, decimal efficiency, decimal pMin, decimal pMax)
        {
            Name = name;
            Type = type;
            Efficiency = efficiency;
            PMin = pMin;
            PMax = pMax;
        }
    }
}
