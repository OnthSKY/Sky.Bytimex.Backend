using FluentAssertions;
using Sky.Template.Backend.Application.Validators.FluentValidation.Products;
using Sky.Template.Backend.Contract.Requests.Products;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Validators;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldPass()
    {
        var request = new CreateProductRequest
        {
            Name = "Test",
            ProductType = "Physical",
            Price = 10,
            Status = "ACTIVE"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WhenNameIsNullOrEmpty_ShouldFail(string? name)
    {
        var request = new CreateProductRequest
        {
            Name = name,
            ProductType = "Physical",
            Price = 10,
            Status = "ACTIVE"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenStatusIsInvalid_ShouldFail()
    {
        var request = new CreateProductRequest
        {
            Name = "Test",
            ProductType = "Physical",
            Price = 10,
            Status = "INVALID"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}

