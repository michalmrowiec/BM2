using AutoMapper;
using BM2.Application.DTOs;
using BM2.Application.Functions.Accounts.Commands.Requests;
using BM2.Application.Functions.Users.Commands.Requests;
using BM2.Application.Functions.Wallets.Commands.Requests;
using BM2.Domain.Entities;
using BM2.Domain.Entities.System;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Currency, CurrencyDTO>();
        
        CreateMap<RecordStatus, RecordStatusDTO>();
        
        CreateMap<AddUserCommand, User>();
        CreateMap<User, UserDTO>();

        CreateMap<AddWalletCommand, Wallet>();
        CreateMap<Wallet, WalletDTO>();
        
        CreateMap<AddAccountCommand, Account>();
        CreateMap<Account, AccountDTO>();
    }
}