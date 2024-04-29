using Core.Dto;
using Core.Models;

namespace Core.Mapper;

public static class Mapper
{
    public static PowerplantModel ToPowerplantModel(PowerplantDto powerplant, decimal merit, decimal cost)
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

    public static PayloadDto ToPayloadDto(Payload payload)
    {
        var payloadDto = new PayloadDto
        {
            Load = payload.Load,
            Fuels = payload.Fuels,
            PowerPlants = payload.PowerPlants,
        };
        return payloadDto;
    }

    public static FuelsDto ToFuelsDto(Fuels fuels)
    {
        var fuelsDto = new FuelsDto
        {

        }
    }

    public static PowerplantDto ToPowerplantDto(Powerplant powerplant)
    {
        var PowerplantDto = new PowerplantDto
        {
            Name = powerplant.Name,
            Type = powerplant.Type,
            Efficiency = powerplant.Efficiency,
            PMin = powerplant.PMin,
            PMax = powerplant.PMax,
        };
        return PowerplantDto;
    }
}
