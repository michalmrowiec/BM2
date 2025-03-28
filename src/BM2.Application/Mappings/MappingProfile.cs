﻿using AutoMapper;
using BM2.Domain.Entities.System;
using BM2.Domain.Entities.UserProfile;
using BM2.Domain.Entities.UserRecords;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Account;
using BM2.Shared.Requests.Commands.Category;
using BM2.Shared.Requests.Commands.Record;
using BM2.Shared.Requests.Commands.Tag;
using BM2.Shared.Requests.Commands.User;
using BM2.Shared.Requests.Commands.Wallet;

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
        CreateMap<Wallet, WalletBaseDTO>();
        CreateMap<Wallet, WalletDTO>();
        CreateMap<UpdateWalletCommand, Wallet>()
            .ForMember(x => x.Id, opt => opt.UseDestinationValue())
            .ForMember(x => x.CreatedAt, opt => opt.UseDestinationValue())
            .ForMember(x => x.CreatedBy, opt => opt.UseDestinationValue())
            .ForMember(x => x.UpdatedAt, opt => opt.Ignore())
            .ForMember(x => x.UpdatedBy, opt => opt.Ignore())
            .ForMember(x => x.DeletedAt, opt => opt.Ignore())
            .ForMember(x => x.DeletedBy, opt => opt.Ignore())
            .ForMember(x => x.OwnedByUserId, opt => opt.UseDestinationValue());

        CreateMap<AddCategoryCommand, Category>()
            .ForSourceMember(src => src.WalletIds, opt => opt.DoNotValidate());
        CreateMap<Category, CategoryDTO>();
        
        CreateMap<AddTagCommand, Tag>()
            .ForSourceMember(src => src.WalletIds, opt => opt.DoNotValidate());
        CreateMap<Tag, TagDTO>();
        
        CreateMap<AddRecordCommand, Record>();
        CreateMap<Record, RecordDTO>();

        CreateMap<UpdateRecordCommand, Record>()
            .ForMember(x => x.Id, opt => opt.UseDestinationValue())
            .ForMember(x => x.CreatedAt, opt => opt.UseDestinationValue())
            .ForMember(x => x.CreatedBy, opt => opt.UseDestinationValue())
            .ForMember(x => x.UpdatedAt, opt => opt.Ignore())
            .ForMember(x => x.UpdatedBy, opt => opt.Ignore())
            .ForMember(x => x.DeletedAt, opt => opt.Ignore())
            .ForMember(x => x.DeletedBy, opt => opt.Ignore())
            .ForMember(x => x.OwnedByUserId, opt => opt.UseDestinationValue());
    }
}