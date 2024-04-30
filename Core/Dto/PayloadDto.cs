using Core.Constants;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dto
{
    public class PayloadDto
    {
        public decimal Load { get; set; }
        public FuelsDto Fuels { get; set; }
        public PowerplantDto[] PowerPlants { get; set; }

        public PayloadDto(decimal load, FuelsDto fuels, PowerplantDto[] powerPlants)
        {
            Load = load;
            Fuels = fuels;
            PowerPlants = powerPlants;
        }

        public IEnumerable<PowerplantModel> GetPowerplantMeritOrder()
        {
            var load = Load;
            var meritOrder = new List<PowerplantModel>();

            foreach (var powerplant in PowerPlants)
            {
                switch (powerplant.Type)
                {
                    case Enum.PlantTypeEnum.windturbine:
                        var powerPlantToAdd = new PowerplantModel(powerplant, 0, 0);
                        powerPlantToAdd.PMax *= Fuels.Wind / (decimal)100.0m;
                        meritOrder.Add(powerPlantToAdd);
                        break;
                    case Enum.PlantTypeEnum.turbojet:
                        meritOrder.Add(new PowerplantModel(powerplant, (Fuels.Kerosine * load) / powerplant.Efficiency, Fuels.Kerosine / powerplant.Efficiency));
                        break;
                    case Enum.PlantTypeEnum.gasfired:
                        meritOrder.Add(new PowerplantModel(powerplant, (Fuels.Gas * load) / powerplant.Efficiency, Fuels.Gas / powerplant.Efficiency));
                        break;
                }
            }

            return meritOrder.OrderBy(m => m.Merit);
        }
    }
}
