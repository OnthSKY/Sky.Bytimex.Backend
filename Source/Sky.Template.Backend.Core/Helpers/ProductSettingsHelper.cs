using System;

namespace Sky.Template.Backend.Core.Helpers;

public class GlobalProductSettings
{
    public bool MaintenanceMode { get; set; }
    public int MaxProductCountPerVendor { get; set; }
    public bool RequireVendorKycForPublishing { get; set; }
    public bool AllowProductDeletion { get; set; } = true;
}

public class VendorProductSettings
{
    public bool? MaintenanceMode { get; set; }
    public int? MaxProductCountPerVendor { get; set; }
    public bool? RequireVendorKycForPublishing { get; set; }
    public bool? AllowProductDeletion { get; set; }
}

public class ProductSettings
{
    public bool MaintenanceMode { get; set; }
    public int MaxProductCountPerVendor { get; set; }
    public bool RequireVendorKycForPublishing { get; set; }
    public bool AllowProductDeletion { get; set; }
}

public static class ProductSettingsHelper
{
    public static ProductSettings BuildEffective(GlobalProductSettings global, VendorProductSettings? vendor)
    {
        if (global == null) throw new ArgumentNullException(nameof(global));
        return new ProductSettings
        {
            MaintenanceMode = vendor?.MaintenanceMode ?? global.MaintenanceMode,
            MaxProductCountPerVendor = vendor?.MaxProductCountPerVendor ?? global.MaxProductCountPerVendor,
            RequireVendorKycForPublishing = vendor?.RequireVendorKycForPublishing ?? global.RequireVendorKycForPublishing,
            AllowProductDeletion = vendor?.AllowProductDeletion ?? global.AllowProductDeletion
        };
    }
}
