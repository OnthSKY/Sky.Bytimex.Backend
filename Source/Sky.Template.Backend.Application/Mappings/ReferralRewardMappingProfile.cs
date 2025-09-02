using AutoMapper;
using Sky.Template.Backend.Contract.Requests.ReferralRewards;
using Sky.Template.Backend.Contract.Responses.ReferralRewardResponses;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Infrastructure.Entities;

namespace Sky.Template.Backend.Application.Mappings;

public class ReferralRewardMappingProfile : Profile
{
    public ReferralRewardMappingProfile()
    {
        CreateMap<CreateReferralRewardRequest, ReferralRewardEntity>()
            .ForMember(dest => dest.RewardStatus, opt => opt.MapFrom(src => (src.RewardStatus ?? ReferralRewardStatus.PENDING).ToString()));

        CreateMap<UpdateReferralRewardRequest, ReferralRewardEntity>()
            .ForMember(dest => dest.RewardStatus, opt => opt.MapFrom(src => src.RewardStatus.ToString()))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ReferralRewardEntity, ReferralRewardResponse>()
            .ForMember(dest => dest.RewardStatus, opt => opt.MapFrom(src => Enum.Parse<ReferralRewardStatus>(src.RewardStatus)));
    }
}
