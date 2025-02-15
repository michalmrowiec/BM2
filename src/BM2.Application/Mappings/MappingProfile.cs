using AutoMapper;
using BM2.Application.DTOs;
using BM2.Application.Functions.Account.Commands.Requests;
using BM2.Application.Functions.Category.Commands.Requests;
using BM2.Application.Functions.Record.Commands.Requests;
using BM2.Application.Functions.Tag.Commands.Requests;
using BM2.Application.Functions.User.Commands.Requests;
using BM2.Application.Functions.Wallet.Commands.Requests;
using BM2.Domain.Entities;
using BM2.Domain.Entities.System;
using BM2.Domain.Entities.UserProfile;
using BM2.Domain.Entities.UserRecords;

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

        CreateMap<AddCategoryCommand, Category>()
            .ForSourceMember(src => src.WalletIds, opt => opt.DoNotValidate());
        CreateMap<Category, CategoryDTO>();
        
        CreateMap<AddTagCommand, Tag>()
            .ForSourceMember(src => src.WalletIds, opt => opt.DoNotValidate());
        CreateMap<Tag, TagDTO>();
        
        CreateMap<AddRecordCommand, Record>();
        CreateMap<Record, RecordDTO>();
    }
}