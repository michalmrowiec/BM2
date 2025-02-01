using AutoMapper;
using BM2.Application.Functions.Dtos;
using BM2.Domain.Entities;

namespace BM2.Application.Mappings;

public class MappingPrifile : Profile
{
    public MappingPrifile()
    {
        CreateMap<User, UserDto>();
    }
}