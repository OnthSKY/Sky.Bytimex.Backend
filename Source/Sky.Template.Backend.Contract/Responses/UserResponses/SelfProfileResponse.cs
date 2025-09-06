namespace Sky.Template.Backend.Contract.Responses.UserResponses;

public sealed class SelfProfileResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? UserImagePath { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime? LastLoginAt { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public RoleDto Role { get; set; } = new();
    public List<string> PermissionCodes { get; set; } = new();
    public VendorSummaryDto? Vendor { get; set; }
    public KycSummaryDto? Kyc { get; set; }
    public List<UserAddressDto> Addresses { get; set; } = new();
    public UserPreferencesDto? Preferences { get; set; }
    public NotificationSettingsDto? Notifications { get; set; }
    public List<UserSessionDto> Sessions { get; set; } = new();
}

public sealed class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class VendorSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string KycStatus { get; set; } = "UNVERIFIED";
    public string Status { get; set; } = "ACTIVE";
}

public sealed class KycSummaryDto
{
    public string KycStatus { get; set; } = "UNVERIFIED";
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastReviewedAt { get; set; }
}

public sealed class UserAddressDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "ACTIVE";
}

public sealed class UserPreferencesDto
{
    public string Language { get; set; } = "en";
    public string Currency { get; set; } = "USD";
    public string Theme { get; set; } = "system";
    public string? TimeZone { get; set; }
}

public sealed class NotificationSettingsDto
{
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool NewsletterOptIn { get; set; }
}

public sealed class UserSessionDto
{
    public Guid Id { get; set; }
    public string Device { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsCurrent { get; set; }
}
