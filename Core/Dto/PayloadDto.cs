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
        // TODO is it really a list that you want.
        public PowerplantDto[] PowerPlants { get; set; }

        public PayloadDto(decimal load, FuelsDto fuels, PowerplantDto[] powerPlants)
        {
            Load = load;
            Fuels = fuels;
            PowerPlants = powerPlants;
        }

        // TODO : no need for methods in a DTO, so differnece between dto/domain object would be best

        public List<PowerplantModel> GetPowerplantMeritOrder()
        {
            var load = this.Load;
            var meritOrder = new List<PowerplantModel>();

            // TODO : possibility to create classes instead.
            foreach (var powerplant in this.PowerPlants)
            {
                // TODO : use enum instead of string
                switch (powerplant.Type.ToLower())
                {
                    case ProjectConstants.Windturbine:
                        var powerPlantToAdd = Mapper.Mapper.ToPowerplantModel(powerplant, 0, 0);
                        powerPlantToAdd.PMax *= Fuels.Wind / (decimal)100.0m;
                        meritOrder.Add(powerPlantToAdd);
                        break;
                    case ProjectConstants.Turbojet:
                        meritOrder.Add(Mapper.Mapper.ToPowerplantModel(powerplant, (Fuels.Kerosine * load) / powerplant.Efficiency, Fuels.Kerosine / powerplant.Efficiency));
                        break;
                    case ProjectConstants.Gasfired:
                        meritOrder.Add(Mapper.Mapper.ToPowerplantModel(powerplant, (Fuels.Gas * load) / powerplant.Efficiency, Fuels.Gas / powerplant.Efficiency));
                        break;
                }
            }

            // TODO see deferred executions and how to return an enumerable with Yield
            return meritOrder.OrderBy(m => m.Merit).ToList();
        }
    }
}
