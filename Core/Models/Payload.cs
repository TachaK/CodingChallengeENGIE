using Core.Constants;

namespace Core.Models
{
    public class Payload
    {
        public decimal Load { get; set; }
        public Fuels Fuels { get; set; }
        public List<Powerplant> PowerPlants { get; set; }

        public Payload(decimal load, Fuels fuels, List<Powerplant> powerPlants)
        {
            Load = load;
            Fuels = fuels;
            PowerPlants = powerPlants;
        }

        public List<PowerplantModel> GetPowerplantMeritOrder()
        {
            var load = this.Load;
            var meritOrder = new List<PowerplantModel>();

            foreach (var powerplant in this.PowerPlants)
            {
                switch (powerplant.Type.ToLower())
                {
                    case ProjectConstants.Windturbine:
                        var powerPlantToAdd = Mapper.Mapper.ToPowerplantModel(powerplant, 0, 0);
                        powerPlantToAdd.PMax *= Fuels.Wind / (decimal)100.0m;
                        meritOrder.Add(powerPlantToAdd);
                        break;
                    case ProjectConstants.Turbojet:
                        meritOrder.Add(Mapper.Mapper.ToPowerplantModel(powerplant, (Fuels.Kerosine * load) / powerplant.Efficiency, Fuels.Kerosine/powerplant.Efficiency));
                        break;
                    case ProjectConstants.Gasfired:
                        meritOrder.Add(Mapper.Mapper.ToPowerplantModel(powerplant, (Fuels.Gas * load) / powerplant.Efficiency, Fuels.Gas/ powerplant.Efficiency));
                        break;
                }
            }
            return meritOrder.OrderBy(m => m.Merit).ToList();
        }
    }

}