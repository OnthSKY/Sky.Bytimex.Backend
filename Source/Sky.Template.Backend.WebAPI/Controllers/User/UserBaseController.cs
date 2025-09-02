using Microsoft.AspNetCore.Authorization;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.User;

[Authorize(Roles = "User")]
public abstract class UserBaseController : CustomBaseController
{
}
