using Core.Dto;
using Core.Interface;
using Core.Models;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Mapper;

public class Mapper : IMapper
{
    public void RegisterMapsterConfiguration()
    {
        TypeAdapterConfig<PowerplantDto, PowerplantModel>
            .NewConfig();
        TypeAdapterConfig<Payload, PayloadDto>
            .NewConfig();
        TypeAdapterConfig<Powerplant, PowerplantDto>
            .NewConfig();
        TypeAdapterConfig<Fuels, FuelsDto>
            .NewConfig();
    }
}
