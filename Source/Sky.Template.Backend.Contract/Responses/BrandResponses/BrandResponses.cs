using System;
using System.Collections.Generic;
using Sky.Template.Backend.Contract.Requests.BrandRequests;
using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.BrandResponses;

public class BrandDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = default!;
    public string? LogoUrl { get; init; }
    public string Status { get; init; } = default!;
    public List<BrandTranslationDto> Translations { get; init; } = new();
}

public class BrandListItemDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = default!;
    public string? LogoUrl { get; init; }
    public string Name { get; init; } = default!;
    public string Status { get; init; } = default!;
}

public class BrandListPaginatedResponse
{
    public PaginatedData<BrandListItemDto> Brands { get; set; } = new();
}
