using Sky.Template.Backend.Application.Services;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class PasswordHashServiceTests
{
    private readonly PasswordHashService _service = new();

    [Fact]
    public void HashPassword_ReturnsHashedValue()
    {
        var password = "Secret123!";
        var hashed = _service.HashPassword(password);

        Assert.False(string.IsNullOrWhiteSpace(hashed));
        Assert.NotEqual(password, hashed);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_ForCorrectPassword()
    {
        var password = "Secret123!";
        var hashed = _service.HashPassword(password);

        var result = _service.VerifyPassword(password, hashed);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForIncorrectPassword()
    {
        var hashed = _service.HashPassword("Secret123!");

        var result = _service.VerifyPassword("OtherPass", hashed);

        Assert.False(result);
    }
}

