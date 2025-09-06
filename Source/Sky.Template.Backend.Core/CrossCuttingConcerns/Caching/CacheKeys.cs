namespace Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

public static class CacheKeys
{
    private const string Prefix = "cache";

    // Genel kaynaklar (sadece prefix, kültür hariç)
    public const string ProductsPrefix = $"{Prefix}:products";
    public const string ProductsPattern = $"{Prefix}:products:*";

    public const string PermissionsPrefix = $"{Prefix}:permissions";
    public const string PermissionsPattern = $"{Prefix}:permissions:*";

    public static string Role(int id, string culture) => $"{Prefix}:roles:{culture}:{id}";
    public const string SelfProfilePrefix = $"{Prefix}:selfprofile";
    public const string SelfProfilePattern = $"{Prefix}:selfprofile:*";
    public const string SelfPermissionsPrefix = $"{Prefix}:selfpermissions";
    public const string SelfPermissionsPattern = $"{Prefix}:selfpermissions:*";

    public const string RolesPrefix = $"{Prefix}:roles";
    public const string RolesPattern = $"{Prefix}:roles:*";

    public const string CategoriesPrefix = $"{Prefix}:categories";
    public const string CategoriesPattern = $"{Prefix}:categories:*";

    public const string ResourcesPrefix = $"{Prefix}:resources";
    public const string ResourcesPattern = $"{Prefix}:resources:*";

    public const string SettingsPrefix = $"{Prefix}:settings";
    public const string SettingsPattern = $"{Prefix}:settings:*";

    public const string VendorOverridesPrefix = $"{Prefix}:vendor-overrides";
    public const string VendorOverridesPattern = $"{Prefix}:vendor-overrides:*";

    // Vendor KYC
    public const string VendorKycPrefix = $"{Prefix}:vendor-kyc";
    public const string VendorKycPattern = $"{Prefix}:vendor-kyc:*";

    // Product settings
    public const string ProductGlobalSettingsPrefix = "settings:product:global";
    public const string ProductVendorSettingsPrefix = "settings:product:vendor";
    public const string ProductVendorSettingsPattern = "settings:product:vendor:*";

    // Kullanıcıya özel keyler culture içerdiği için sabit yapamayız (method olarak kalmalı)
    public const string UserResourcesPrefix = $"{Prefix}:userresources";
    public static string UserResources(Guid userId, string culture) => $"{Prefix}:userresources:{culture}:{userId}";
    public static string UserResourcesPattern(Guid userId) => $"{Prefix}:userresources:*:{userId}";

    // Alıcılar
    public const string BuyersPrefix = $"{Prefix}:buyers";
    public const string BuyersPattern = $"{Prefix}:buyers:*";

    // Buyer Addresses
    public const string BuyerAddressesPrefix = $"{Prefix}:buyeraddresses";
    public const string BuyerAddressesPattern = $"{Prefix}:buyeraddresses:*";

    // Carts
    public const string CartsPrefix = $"{Prefix}:carts";
    public const string CartsPattern = $"{Prefix}:carts:*";

    public const string CartsServiceGetPrefix = $"{Prefix}:cartservice:get";
    public const string CartsServiceGetPattern = $"{Prefix}:cartservice:get*";

    // Cart Items
    public const string CartItemsPrefix = $"{Prefix}:cartitems";
    public const string CartItemsPattern = $"{Prefix}:cartitems:*";

    public const string CartItemsServiceGetPrefix = $"{Prefix}:cartitemservice:get";
    public const string CartItemsServiceGetPattern = $"{Prefix}:cartitemservice:get*";

    // Payments
    public const string PaymentsPrefix = $"{Prefix}:payments";
    public const string PaymentsPattern = $"{Prefix}:payments:*";

    public const string PaymentsServiceGetPrefix = $"{Prefix}:paymentservice:get";
    public const string PaymentsServiceGetPattern = $"{Prefix}:paymentservice:get*";

    // Payment Methods
    public const string PaymentMethodsPrefix = $"{Prefix}:paymentmethods";
    public const string PaymentMethodsPattern = $"{Prefix}:paymentmethods:*";

    public const string PaymentMethodsServiceGetPrefix = $"{Prefix}:paymentmethodservice:get";
    public const string PaymentMethodsServiceGetPattern = $"{Prefix}:paymentmethodservice:get*";

    // Invoices
    public const string InvoicesPrefix = $"{Prefix}:invoices";
    public const string InvoicesPattern = $"{Prefix}:invoices:*";

    public const string InvoicesServiceGetPrefix = $"{Prefix}:invoiceservice:get";
    public const string InvoicesServiceGetPattern = $"{Prefix}:invoiceservice:get*";

    // Shipments
    public const string ShipmentsPrefix = $"{Prefix}:shipments";
    public const string ShipmentsPattern = $"{Prefix}:shipments:*";

    public const string ShipmentsServiceGetPrefix = $"{Prefix}:shipmentservice:get";
    public const string ShipmentsServiceGetPattern = $"{Prefix}:shipmentservice:get*";

 
    // Servis özel keyler
    public const string BuyersServiceGetPrefix = $"{Prefix}:buyerservice:get";
    public const string BuyersServiceGetPattern = $"{Prefix}:buyerservice:get*";

    public const string BuyerAddressesServiceGetPrefix = $"{Prefix}:buyeraddresseservice:get";
    public const string BuyerAddressesServiceGetPattern = $"{Prefix}:buyeraddresseservice:get*";

    // Discounts
    public const string DiscountsPrefix = $"{Prefix}:discounts";
    public const string DiscountsPattern = $"{Prefix}:discounts:*";

    // Kyc
    public const string UserKycPrefix = $"{Prefix}:userkyc";
    public const string UserKycPattern = $"{Prefix}:userkyc:*";

    // Stock status
    public const string StockStatusPrefix = $"{Prefix}:stockstatus";
    public const string StockStatusPattern = $"{Prefix}:stockstatus:*";

    public const string BrandsPrefix = $"{Prefix}:brands";
    public const string BrandsPattern = $"{Prefix}:brands:*";
}