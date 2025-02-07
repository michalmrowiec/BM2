using AutoMapper;
using BM2.Application.DTOs;
using BM2.Application.Functions.Users.Commands.AddUserCommand;
using BM2.Application.Functions.Wallets.Commands.AddWalletCommand;
using BM2.Domain.Entities;
using BM2.Domain.Entities.System;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Application.Mappings;

public class MappingPrifile : Profile
{
    public MappingPrifile()
    {
        CreateMap<Currency, CurrencyDTO>();
        
        CreateMap<AddUserCommand, User>();
        CreateMap<User, UserDTO>();

        CreateMap<AddWalletCommand, Wallet>();
        CreateMap<Wallet, WalletDTO>();
    }
}