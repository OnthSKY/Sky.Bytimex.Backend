using Sky.Template.Backend.Core.Models;

namespace Sky.Template.Backend.Contract.Responses.RoleResponses;

public class RolePageResponse
{
    public List<RoleDto> Roles { get; set; } = [];
    public int RoleTotalCount => Roles.Count;
}

public class RoleDto : Role
{
    public int TotalUserCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }
}
