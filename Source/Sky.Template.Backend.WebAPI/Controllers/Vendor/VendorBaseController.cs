using Microsoft.AspNetCore.Authorization;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.Vendor;

[Authorize(Roles = "Vendor")]
public abstract class VendorBaseController : CustomBaseController
{
}
