using System;
using FluentAssertions;
using Sky.Template.Backend.Application.Validators.FluentValidation.Shipments;
using Sky.Template.Backend.Contract.Requests.Shipments;
using Sky.Template.Backend.Core.Enums;
using Xunit;

namespace Sky.Template.Backend.UnitTests.Validators;

public class CreateShipmentRequestValidatorTests
{
    private readonly CreateShipmentRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldPass()
    {
        var request = new CreateShipmentRequest
        {
            OrderId = Guid.NewGuid(),
            ShipmentDate = DateTime.UtcNow,
            Carrier = "UPS",
            TrackingNumber = "TRACK123",
            Status = ShipmentStatus.PREPARING.ToString()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenOrderIdIsEmpty_ShouldFail()
    {
        var request = new CreateShipmentRequest
        {
            OrderId = Guid.Empty,
            ShipmentDate = DateTime.UtcNow,
            Carrier = "UPS",
            TrackingNumber = "TRACK123",
            Status = ShipmentStatus.PREPARING.ToString()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenStatusIsInvalid_ShouldFail()
    {
        var request = new CreateShipmentRequest
        {
            OrderId = Guid.NewGuid(),
            ShipmentDate = DateTime.UtcNow,
            Carrier = "UPS",
            TrackingNumber = "TRACK123",
            Status = "INVALID"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}

