using Core.Enum;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Core.Dto
{
    public class PowerplantDto
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PlantTypeEnum Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PMin { get; set; }
        public decimal PMax { get; set; }

        public PowerplantDto(string name, PlantTypeEnum type, decimal efficiency, decimal pMin, decimal pMax)
        {
            Name = name;
            Type = type;
            Efficiency = efficiency;
            PMin = pMin;
            PMax = pMax;
        }
    }
}
