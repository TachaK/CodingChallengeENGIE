
using Core.Dto;
using Core.Enum;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Core.Models
{
    public class PowerplantModel
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]

        public PlantTypeEnum Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PMin { get; set; }
        public decimal PMax { get; set; }
        public decimal Merit { get; set; }
        public decimal Cost { get; set; }

        public PowerplantModel(PowerplantDto plant, decimal merit, decimal cost)
        {
            Name = plant.Name;
            Type = plant.Type;
            Efficiency = plant.Efficiency;
            PMin = plant.PMin;
            PMax = plant.PMax;
            Merit = merit;
            Cost = cost;
        }
    }
}
