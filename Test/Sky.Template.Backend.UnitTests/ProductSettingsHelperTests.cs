using FluentAssertions;
using Sky.Template.Backend.Core.Helpers;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class ProductSettingsHelperTests
{
    [Fact]
    public void VendorOverridesGlobal_WhenProvided()
    {
        var global = new GlobalProductSettings
        {
            MaintenanceMode = false,
            MaxProductCountPerVendor = 5,
            RequireVendorKycForPublishing = false,
            AllowProductDeletion = true
        };
        var vendor = new VendorProductSettings
        {
            MaintenanceMode = true,
            MaxProductCountPerVendor = 10,
            RequireVendorKycForPublishing = true,
            AllowProductDeletion = false
        };

        var effective = ProductSettingsHelper.BuildEffective(global, vendor);
        effective.MaintenanceMode.Should().BeTrue();
        effective.MaxProductCountPerVendor.Should().Be(10);
        effective.RequireVendorKycForPublishing.Should().BeTrue();
        effective.AllowProductDeletion.Should().BeFalse();
    }

    [Fact]
    public void UsesGlobal_WhenVendorValuesNull()
    {
        var global = new GlobalProductSettings
        {
            MaintenanceMode = true,
            MaxProductCountPerVendor = 3,
            RequireVendorKycForPublishing = false,
            AllowProductDeletion = true
        };
        var vendor = new VendorProductSettings();

        var effective = ProductSettingsHelper.BuildEffective(global, vendor);
        effective.MaintenanceMode.Should().BeTrue();
        effective.MaxProductCountPerVendor.Should().Be(3);
        effective.RequireVendorKycForPublishing.Should().BeFalse();
        effective.AllowProductDeletion.Should().BeTrue();
    }
}
