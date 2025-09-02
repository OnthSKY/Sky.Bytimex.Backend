using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.User;

public class DelegationEntity : BaseEntity<int>
{
    [DbManager.mColumn("user_id")] public int UserId { get; set; }
    [DbManager.mColumn("delegated_user_id")] public int DelegatedUserId { get; set; }
    [DbManager.mColumn("delegated_user_fullname")] public string DelegatedUserFullName { get; set; }
    [DbManager.mColumn("delegated_user_email")] public string DelegatedUserEmail { get; set; }
    [DbManager.mColumn("delegated_user_image")] public string DelegatedUserImagePath { get; set; }
    [DbManager.mColumn("delegation_type")] public string DelegationType { get; set; }
    [DbManager.mColumn("start_date")] public DateTime StartDate { get; set; }
    [DbManager.mColumn("end_date")] public DateTime EndDate { get; set; }
    [DbManager.mColumn("status")] public bool Status { get; set; }
}
