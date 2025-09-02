using System.Collections.Generic;

using System.Security.Claims;

namespace Sky.Template.Backend.UnitTests.Common;

public static class TestClaimsPrincipalFactory
{
    public static ClaimsPrincipal CreateDefault(params Claim[] extraClaims)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000001"),
            new Claim(ClaimTypes.Email, "test.user@local"),
            new Claim(ClaimTypes.Name, "test.user@local")
        };
        claims.AddRange(extraClaims);
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }
}
