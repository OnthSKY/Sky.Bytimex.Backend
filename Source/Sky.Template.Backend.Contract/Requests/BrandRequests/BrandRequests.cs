using System;
using System.Collections.Generic;
using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.BrandRequests;

public record BrandTranslationDto(string LanguageCode, string Name, string? Description);

public class BrandFilterRequest : GridRequest
{
}

public class CreateBrandRequest
{
    public string Code { get; init; } = default!;
    public string? LogoUrl { get; init; }
    public List<BrandTranslationDto> Translations { get; init; } = new();
}

public record UpdateBrandRequest
{
    public Guid Id { get; init; }
    public string Code { get; init; } = default!;
    public string? LogoUrl { get; init; }
    public string Status { get; init; } = "ACTIVE";
    public List<BrandTranslationDto> Translations { get; init; } = new();
}
