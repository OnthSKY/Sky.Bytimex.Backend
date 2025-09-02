using Microsoft.AspNetCore.Authorization;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.Admin;

[Authorize(Roles = "Admin")]
public abstract class AdminBaseController : CustomBaseController
{
}
