using AutoMapper;
using Sky.Template.Backend.Contract.Requests.BrandRequests;
using Sky.Template.Backend.Contract.Responses.BrandResponses;
using Sky.Template.Backend.Infrastructure.Entities.Brand;

namespace Sky.Template.Backend.Application.Mappings;

public class BrandMappingProfile : Profile
{
    public BrandMappingProfile()
    {
        CreateMap<BrandTranslationEntity, BrandTranslationDto>().ReverseMap();
        CreateMap<BrandEntity, BrandDto>()
            .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations));
        CreateMap<BrandListItemEntity, BrandListItemDto>();
    }
}
