using Microsoft.AspNetCore.Authorization;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[Authorize(Roles = "USER")]
public abstract class UserBaseController : CustomBaseController
{
}
