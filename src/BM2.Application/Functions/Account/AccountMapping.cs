using AutoMapper;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Account;

namespace BM2.Application.Functions.Account;

public class AccountMapping : Profile
{
    public AccountMapping()
    {
        CreateMap<AddUpdateAccountCommand, Domain.Entities.UserProfile.Account>()
            .ForMember(x => x.Id, opt => opt.UseDestinationValue())
            .ForMember(x => x.CreatedAt, opt => opt.UseDestinationValue())
            .ForMember(x => x.CreatedBy, opt => opt.UseDestinationValue())
            .ForMember(x => x.UpdatedAt, opt => opt.Ignore())
            .ForMember(x => x.UpdatedBy, opt => opt.Ignore())
            .ForMember(x => x.DeletedAt, opt => opt.Ignore())
            .ForMember(x => x.DeletedBy, opt => opt.Ignore())
            .ForMember(x => x.OwnedByUserId, opt => opt.UseDestinationValue());

        CreateMap<Domain.Entities.UserProfile.Account, AccountBasicDTO>();
        CreateMap<Domain.Entities.UserProfile.Account, AccountDTO>();
    }
}