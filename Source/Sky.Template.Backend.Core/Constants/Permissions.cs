namespace Sky.Template.Backend.Core.Constants;
public static class Permissions
{
    public static class AdminReports
    {
        public const string View = "ADMIN_REPORTS.VIEW";
    }
    public static class BuyerAddresses
    {
        public const string Create = "BUYERADDRESSES.CREATE";
        public const string Delete = "BUYERADDRESSES.DELETE";
        public const string Update = "BUYERADDRESSES.UPDATE";
        public const string View = "BUYERADDRESSES.VIEW";
    }
    public static class Buyers
    {
        public const string Create = "BUYERS.CREATE";
        public const string Delete = "BUYERS.DELETE";
        public const string Read = "BUYERS.READ";
        public const string Update = "BUYERS.UPDATE";
    }
    public static class CartItems
    {
        public const string Create = "CART_ITEMS.CREATE";
        public const string Delete = "CART_ITEMS.DELETE";
        public const string Update = "CART_ITEMS.UPDATE";
        public const string View = "CART_ITEMS.VIEW";
    }
    public static class Carts
    {
        public const string Create = "CARTS.CREATE";
        public const string Delete = "CARTS.DELETE";
        public const string Update = "CARTS.UPDATE";
        public const string View = "CARTS.VIEW";
    }
    public static class Common
    {
        public const string HardDelete = "COMMON.HARD_DELETE";
    }
    public static class Dashboard
    {
        public static class User
        {
            public const string Read = "DASHBOARD.USER_READ";
        }
        public static class Vendor
        {
            public const string Read = "DASHBOARD.VENDOR_READ";
        }
    }
    public static class DiscountUsages
    {
        public const string Create = "DISCOUNTUSAGES.CREATE";
        public const string View = "DISCOUNTUSAGES.VIEW";
    }
    public static class Discounts
    {
        public const string Apply = "DISCOUNTS.APPLY";
        public const string Create = "DISCOUNTS.CREATE";
        public const string Delete = "DISCOUNTS.DELETE";
        public const string Update = "DISCOUNTS.UPDATE";
        public const string View = "DISCOUNTS.VIEW";
    }
    public static class ErrorLogs
    {
        public const string View = "ERROR_LOGS.VIEW";
    }
    public static class FileUploads
    {
        public const string Create = "FILEUPLOADS.CREATE";
        public const string Delete = "FILEUPLOADS.DELETE";
        public const string Update = "FILEUPLOADS.UPDATE";
        public const string View = "FILEUPLOADS.VIEW";
    }
    public static class Invoices
    {
        public const string Create = "INVOICES.CREATE";
        public const string Delete = "INVOICES.DELETE";
        public const string Update = "INVOICES.UPDATE";
        public const string View = "INVOICES.VIEW";
    }
    public static class Kyc
    {
        public const string Delete = "KYC.DELETE";
        public const string Resubmit = "KYC.RESUBMIT";
        public const string Verify = "KYC.VERIFY";
        public const string Manage = "KYC.MANAGE";
    }
    public static class OrderDetails
    {
        public const string Create = "ORDERDETAILS.CREATE";
        public const string Delete = "ORDERDETAILS.DELETE";
        public const string Update = "ORDERDETAILS.UPDATE";
        public const string View = "ORDERDETAILS.VIEW";
    }
    public static class OrderStatusLogs
    {
        public const string Create = "ORDERSTATUSLOGS.CREATE";
        public const string View = "ORDERSTATUSLOGS.VIEW";
    }
    public static class Orders
    {
        public const string Cancel = "ORDERS.CANCEL";
        public const string Create = "ORDERS.CREATE";
        public const string Delete = "ORDERS.DELETE";
        public const string Reorder = "ORDERS.REORDER";
        public const string Update = "ORDERS.UPDATE";
        public const string View = "ORDERS.VIEW";
    }
    public static class PaymentMethods
    {
        public const string Create = "PAYMENT_METHODS.CREATE";
        public const string Delete = "PAYMENT_METHODS.DELETE";
        public const string Update = "PAYMENT_METHODS.UPDATE";
        public const string View = "PAYMENT_METHODS.VIEW";
    }
    public static class Payments
    {
        public const string Create = "PAYMENTS.CREATE";
        public const string Delete = "PAYMENTS.DELETE";
        public const string Read = "PAYMENTS.READ";
        public const string Update = "PAYMENTS.UPDATE";
        public const string View = "PAYMENTS.VIEW";
    }
    public static class PermissionModule
    {
        public const string Create = "PERMISSIONS.CREATE";
        public const string Delete = "PERMISSIONS.DELETE";
        public const string Update = "PERMISSIONS.UPDATE";
        public const string View = "PERMISSIONS.VIEW";
    }
    public static class Products
    {
        public const string Create = "PRODUCTS.CREATE";
        public const string Delete = "PRODUCTS.DELETE";
        public const string HardDelete = "PRODUCTS.HARD_DELETE";
        public const string Read = "PRODUCTS.READ";
        public const string Update = "PRODUCTS.UPDATE";
    }
    public static class ReferralRewards
    {
        public const string Create = "REFERRAL_REWARDS.CREATE";
        public const string Delete = "REFERRAL_REWARDS.DELETE";
        public const string Update = "REFERRAL_REWARDS.UPDATE";
        public const string View = "REFERRAL_REWARDS.VIEW";
    }
    public static class Returns
    {
        public const string Create = "RETURNS.CREATE";
        public const string Delete = "RETURNS.DELETE";
        public const string Read = "RETURNS.READ";
        public const string Update = "RETURNS.UPDATE";
    }
    public static class Roles
    {
        public const string Create = "ROLES.CREATE";
        public const string Delete = "ROLES.DELETE";
        public const string PermissionAdd = "ROLES.PERMISSION_ADD";
        public const string Update = "ROLES.UPDATE";
        public const string View = "ROLES.VIEW";
    }
    public static class Settings
    {
        public const string Manage = "SETTINGS.MANAGE";
        public const string Override = "SETTINGS.OVERRIDE";
    }
    public static class Shipments
    {
        public const string All = "SHIPMENTS.ALL";
        public const string Create = "SHIPMENTS.CREATE";
        public const string Delete = "SHIPMENTS.DELETE";
        public const string Update = "SHIPMENTS.UPDATE";
        public const string View = "SHIPMENTS.VIEW";
    }
    public static class StockMovements
    {
        public const string Create = "STOCKMOVEMENTS.CREATE";
        public const string Delete = "STOCKMOVEMENTS.DELETE";
        public const string Update = "STOCKMOVEMENTS.UPDATE";
        public const string View = "STOCKMOVEMENTS.VIEW";
    }
    public static class Suppliers
    {
        public const string Delete = "SUPPLIERS.DELETE";
        public const string Edit = "SUPPLIERS.EDIT";
        public const string View = "SUPPLIERS.VIEW";
    }
    public static class Users
    {
        public const string RoleChange = "USERS.ROLE_CHANGE";
        public const string SoftDelete = "USERS.SOFT_DELETE";
        public const string Update = "USERS.UPDATE";
    }
    public static class Vendors
    {
        public const string All = "VENDORS.ALL";
        public const string Create = "VENDORS.CREATE";
        public const string Delete = "VENDORS.DELETE";
        public const string SoftDelete = "VENDORS.SOFT_DELETE";
        public const string Update = "VENDORS.UPDATE";
        public const string UpdateSelf = "VENDORS.UPDATE_SELF";
        public const string Verify = "VENDORS.VERIFY";
        public const string View = "VENDORS.VIEW";
    }

    public static class VendorKyc
    {
        public const string Submit = "VENDOR.KYC.SUBMIT";
        public const string View = "VENDOR.KYC.VIEW";
        public const string Review = "VENDOR.KYC.REVIEW";
    }
    public static class Brands
    {
        public const string View = "BRANDS.VIEW";
        public const string Create = "BRANDS.CREATE";
        public const string Update = "BRANDS.UPDATE";
        public const string Delete = "BRANDS.DELETE";
        public const string HardDelete = "BRANDS.HARD_DELETE";
        public const string VendorCreate = "BRANDS.VENDOR.CREATE";
    }
}
