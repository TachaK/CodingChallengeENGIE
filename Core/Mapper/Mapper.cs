using Core.Models;

namespace Core.Mapper;

public static class Mapper
{
    public static PowerplantModel ToPowerplantModel(Powerplant powerplant, decimal merit, decimal cost)
    {
        var powerplantModel = new PowerplantModel
        {
            Name = powerplant.Name,
            Type = powerplant.Type,
            Efficiency = powerplant.Efficiency,
            PMin = powerplant.PMin,
            PMax = powerplant.PMax,
            Merit = merit,
            Cost = cost
        };
        return powerplantModel;
    }
}
