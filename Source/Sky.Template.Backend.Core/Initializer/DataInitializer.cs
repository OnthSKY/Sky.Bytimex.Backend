using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Transactions;

public static class DataInitializer
{
    public static void SeedData(IConfiguration configuration)
    {
        var provider = configuration["DatabaseConnection:Provider"];
        if (provider != "PostgreSql") return;

        var connectionString = configuration["DatabaseConnection:ConnectionString"];
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var systemUserId = "00000000-0000-0000-0000-000000000001";

        using var tx = connection.BeginTransaction();
        try
        {
            // 1) SYSTEM USER (FK için önce ekle)
            using (var cmd = new NpgsqlCommand(@"
                                    INSERT INTO sys.users
                                    (id, username, first_name, last_name, email, phone, password_hash, kyc_status, is_email_verified, status, created_at)
                                    VALUES
                                    (@id, @username, 'System', 'Administrator', 'system@skytemplate.com', @phone, @pwd, 'VERIFIED', TRUE, 'ACTIVE', now())
                                    ON CONFLICT (id) DO NOTHING;
                                ", connection, tx))
                                            {
                                                cmd.Parameters.AddWithValue("@id", Guid.Parse(systemUserId));
                                                cmd.Parameters.AddWithValue("@username", "systemadmin");
                                                cmd.Parameters.AddWithValue("@phone", "5550000000");  // unique olmalı
                                                cmd.Parameters.AddWithValue("@pwd", "seedhash");     // örnek
                                                cmd.ExecuteNonQuery();
                                            }

            // 2) ROLES
            using (var cmd = new NpgsqlCommand(@"INSERT INTO sys.roles (name, description, status, created_at) VALUES
                                                ('SYSTEM_ADMIN', 'System administrator role', 'ACTIVE', now()),
                                                ('USER', 'User role', 'ACTIVE', now()),
                                                ('VENDOR', 'Satıcı kullanıcı rolü', 'ACTIVE', now()),
                                                ('ACCOUNTANT', 'Muhasebe rolü', 'ACTIVE', now()),
                                                ('SUPPORT', 'Destek ekibi rolü', 'ACTIVE', now())
                                               ON CONFLICT (name) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // 3) RESOURCES (tam liste)
            using (var cmd = new NpgsqlCommand(@"INSERT INTO sys.resources (code, name, description)
              VALUES
                ('ADMIN_REPORTS', 'ADMIN_REPORTS', 'ADMIN_REPORTS operations'),
                ('BUYERADDRESSES', 'BUYERADDRESSES', 'BUYERADDRESSES operations'),
                ('BUYERS', 'BUYERS', 'BUYERS operations'),
                ('CARTS', 'CARTS', 'CARTS operations'),
                ('CART_ITEMS', 'CART_ITEMS', 'CART_ITEMS operations'),
                ('COMMON', 'COMMON', 'COMMON operations'),
                ('DASHBOARD', 'DASHBOARD', 'DASHBOARD operations'),
                ('DISCOUNTS', 'DISCOUNTS', 'DISCOUNTS operations'),
                ('DISCOUNTUSAGES', 'DISCOUNTUSAGES', 'DISCOUNTUSAGES operations'),
                ('ERROR_LOGS', 'ERROR_LOGS', 'ERROR_LOGS operations'),
                ('FILEUPLOADS', 'FILEUPLOADS', 'FILEUPLOADS operations'),
                ('INVOICES', 'INVOICES', 'INVOICES operations'),
                ('KYC', 'KYC', 'KYC operations'),
                ('ORDERDETAILS', 'ORDERDETAILS', 'ORDERDETAILS operations'),
                ('ORDERS', 'ORDERS', 'ORDERS operations'),
                ('ORDERSTATUSLOGS', 'ORDERSTATUSLOGS', 'ORDERSTATUSLOGS operations'),
                ('PAYMENTS', 'PAYMENTS', 'PAYMENTS operations'),
                ('PAYMENT_METHODS', 'PAYMENT_METHODS', 'PAYMENT_METHODS operations'),
                ('PERMISSIONS', 'PERMISSIONS', 'PERMISSIONS operations'),
                ('PRODUCTS', 'PRODUCTS', 'PRODUCTS operations'),
                ('BRANDS', 'BRANDS', 'BRANDS operations'),
                ('REFERRAL_REWARDS', 'REFERRAL_REWARDS', 'REFERRAL_REWARDS operations'),
                ('RETURNS', 'RETURNS', 'RETURNS operations'),
                ('ROLES', 'ROLES', 'ROLES operations'),
                ('SETTINGS', 'SETTINGS', 'SETTINGS operations'),
                ('SHIPMENTS', 'SHIPMENTS', 'SHIPMENTS operations'),
                ('STOCKMOVEMENTS', 'STOCKMOVEMENTS', 'STOCKMOVEMENTS operations'),
                ('SUPPLIERS', 'SUPPLIERS', 'SUPPLIERS operations'),
                ('USERS', 'USERS', 'USERS operations'),
                ('VENDORS', 'VENDORS', 'VENDORS operations')
              ON CONFLICT (code) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // 4) SYSTEM SETTINGS (genişletilmiş)
            using (var cmd = new NpgsqlCommand(@"INSERT INTO sys.system_settings (key, group_name, value, description, is_public, created_at)
              VALUES
                ('MAX_PRODUCT_COUNT_PER_VENDOR','LIMITS','100','Max number of products per vendor', FALSE, now()),
                ('MAINTENANCE_MODE','GENERAL','false','Global system lock', FALSE, now()),
                ('REQUIRE_VENDOR_KYC_FOR_PUBLISHING','COMPLIANCE','true','Force KYC before publishing products', FALSE, now()),
                ('ENABLE_REFERRAL_PROGRAM','MARKETING','true','Enable referral-based bonuses', FALSE, now()),
                ('DEFAULT_TAX_RATE','TAX','18','Default VAT rate (%)', FALSE, now()),
                ('APP_VERSION','GENERAL','1.0.0','Current frontend version', TRUE, now()),
                ('MAX_ORDERS_PER_USER','LIMITS','50','Max active orders per user', FALSE, now())
              ON CONFLICT (key) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // 5) PERMISSIONS (uzun liste – created_by = systemUserId)
            using (var cmd = new NpgsqlCommand($@"INSERT INTO sys.permissions (code, name, description, group_name, created_at, created_by) VALUES
                ('ADMIN_REPORTS.VIEW', 'ADMIN_REPORTS.VIEW', '', 'ADMIN_REPORTS', now(), '{systemUserId}'),
                ('BUYERADDRESSES.CREATE', 'BUYERADDRESSES.CREATE', '', 'BUYERADDRESSES', now(), '{systemUserId}'),
                ('BUYERADDRESSES.DELETE', 'BUYERADDRESSES.DELETE', '', 'BUYERADDRESSES', now(), '{systemUserId}'),
                ('BUYERADDRESSES.UPDATE', 'BUYERADDRESSES.UPDATE', '', 'BUYERADDRESSES', now(), '{systemUserId}'),
                ('BUYERADDRESSES.VIEW', 'BUYERADDRESSES.VIEW', '', 'BUYERADDRESSES', now(), '{systemUserId}'),
                ('BUYERS.CREATE', 'BUYERS.CREATE', '', 'BUYERS', now(), '{systemUserId}'),
                ('BUYERS.DELETE', 'BUYERS.DELETE', '', 'BUYERS', now(), '{systemUserId}'),
                ('BUYERS.READ', 'BUYERS.READ', '', 'BUYERS', now(), '{systemUserId}'),
                ('BUYERS.UPDATE', 'BUYERS.UPDATE', '', 'BUYERS', now(), '{systemUserId}'),
                ('CARTS.CREATE', 'CARTS.CREATE', '', 'CARTS', now(), '{systemUserId}'),
                ('CARTS.DELETE', 'CARTS.DELETE', '', 'CARTS', now(), '{systemUserId}'),
                ('CARTS.UPDATE', 'CARTS.UPDATE', '', 'CARTS', now(), '{systemUserId}'),
                ('CARTS.VIEW', 'CARTS.VIEW', '', 'CARTS', now(), '{systemUserId}'),
                ('CART_ITEMS.CREATE', 'CART_ITEMS.CREATE', '', 'CART_ITEMS', now(), '{systemUserId}'),
                ('CART_ITEMS.DELETE', 'CART_ITEMS.DELETE', '', 'CART_ITEMS', now(), '{systemUserId}'),
                ('CART_ITEMS.UPDATE', 'CART_ITEMS.UPDATE', '', 'CART_ITEMS', now(), '{systemUserId}'),
                ('CART_ITEMS.VIEW', 'CART_ITEMS.VIEW', '', 'CART_ITEMS', now(), '{systemUserId}'),
                ('COMMON.HARD_DELETE', 'COMMON.HARD_DELETE', '', 'COMMON', now(), '{systemUserId}'),
                ('DASHBOARD.USER_READ', 'DASHBOARD.USER_READ', '', 'DASHBOARD', now(), '{systemUserId}'),
                ('DASHBOARD.VENDOR_READ', 'DASHBOARD.VENDOR_READ', '', 'DASHBOARD', now(), '{systemUserId}'),
                ('DISCOUNTS.APPLY', 'DISCOUNTS.APPLY', '', 'DISCOUNTS', now(), '{systemUserId}'),
                ('DISCOUNTS.CREATE', 'DISCOUNTS.CREATE', '', 'DISCOUNTS', now(), '{systemUserId}'),
                ('DISCOUNTS.DELETE', 'DISCOUNTS.DELETE', '', 'DISCOUNTS', now(), '{systemUserId}'),
                ('DISCOUNTS.UPDATE', 'DISCOUNTS.UPDATE', '', 'DISCOUNTS', now(), '{systemUserId}'),
                ('DISCOUNTS.VIEW', 'DISCOUNTS.VIEW', '', 'DISCOUNTS', now(), '{systemUserId}'),
                ('DISCOUNTUSAGES.CREATE', 'DISCOUNTUSAGES.CREATE', '', 'DISCOUNTUSAGES', now(), '{systemUserId}'),
                ('DISCOUNTUSAGES.VIEW', 'DISCOUNTUSAGES.VIEW', '', 'DISCOUNTUSAGES', now(), '{systemUserId}'),
                ('ERROR_LOGS.VIEW', 'ERROR_LOGS.VIEW', '', 'ERROR_LOGS', now(), '{systemUserId}'),
                ('FILEUPLOADS.CREATE', 'FILEUPLOADS.CREATE', '', 'FILEUPLOADS', now(), '{systemUserId}'),
                ('FILEUPLOADS.DELETE', 'FILEUPLOADS.DELETE', '', 'FILEUPLOADS', now(), '{systemUserId}'),
                ('FILEUPLOADS.UPDATE', 'FILEUPLOADS.UPDATE', '', 'FILEUPLOADS', now(), '{systemUserId}'),
                ('FILEUPLOADS.VIEW', 'FILEUPLOADS.VIEW', '', 'FILEUPLOADS', now(), '{systemUserId}'),
                ('INVOICES.CREATE', 'INVOICES.CREATE', '', 'INVOICES', now(), '{systemUserId}'),
                ('INVOICES.DELETE', 'INVOICES.DELETE', '', 'INVOICES', now(), '{systemUserId}'),
                ('INVOICES.UPDATE', 'INVOICES.UPDATE', '', 'INVOICES', now(), '{systemUserId}'),
                ('INVOICES.VIEW', 'INVOICES.VIEW', '', 'INVOICES', now(), '{systemUserId}'),
                ('KYC.DELETE', 'KYC.DELETE', '', 'KYC', now(), '{systemUserId}'),
                ('KYC.MANAGE', 'KYC.MANAGE', '', 'KYC', now(), '{systemUserId}'),
                ('KYC.RESUBMIT', 'KYC.RESUBMIT', '', 'KYC', now(), '{systemUserId}'),
                ('KYC.VERIFY', 'KYC.VERIFY', '', 'KYC', now(), '{systemUserId}'),
                ('ORDERDETAILS.CREATE', 'ORDERDETAILS.CREATE', '', 'ORDERDETAILS', now(), '{systemUserId}'),
                ('ORDERDETAILS.DELETE', 'ORDERDETAILS.DELETE', '', 'ORDERDETAILS', now(), '{systemUserId}'),
                ('ORDERDETAILS.UPDATE', 'ORDERDETAILS.UPDATE', '', 'ORDERDETAILS', now(), '{systemUserId}'),
                ('ORDERDETAILS.VIEW', 'ORDERDETAILS.VIEW', '', 'ORDERDETAILS', now(), '{systemUserId}'),
                ('ORDERS.CANCEL', 'ORDERS.CANCEL', '', 'ORDERS', now(), '{systemUserId}'),
                ('ORDERS.CREATE', 'ORDERS.CREATE', '', 'ORDERS', now(), '{systemUserId}'),
                ('ORDERS.DELETE', 'ORDERS.DELETE', '', 'ORDERS', now(), '{systemUserId}'),
                ('ORDERS.REORDER', 'ORDERS.REORDER', '', 'ORDERS', now(), '{systemUserId}'),
                ('ORDERS.UPDATE', 'ORDERS.UPDATE', '', 'ORDERS', now(), '{systemUserId}'),
                ('ORDERS.VIEW', 'ORDERS.VIEW', '', 'ORDERS', now(), '{systemUserId}'),
                ('ORDERSTATUSLOGS.CREATE', 'ORDERSTATUSLOGS.CREATE', '', 'ORDERSTATUSLOGS', now(), '{systemUserId}'),
                ('ORDERSTATUSLOGS.VIEW', 'ORDERSTATUSLOGS.VIEW', '', 'ORDERSTATUSLOGS', now(), '{systemUserId}'),
                ('PAYMENTS.CREATE', 'PAYMENTS.CREATE', '', 'PAYMENTS', now(), '{systemUserId}'),
                ('PAYMENTS.DELETE', 'PAYMENTS.DELETE', '', 'PAYMENTS', now(), '{systemUserId}'),
                ('PAYMENTS.READ', 'PAYMENTS.READ', '', 'PAYMENTS', now(), '{systemUserId}'),
                ('PAYMENTS.UPDATE', 'PAYMENTS.UPDATE', '', 'PAYMENTS', now(), '{systemUserId}'),
                ('PAYMENTS.VIEW', 'PAYMENTS.VIEW', '', 'PAYMENTS', now(), '{systemUserId}'),
                ('PAYMENT_METHODS.CREATE', 'PAYMENT_METHODS.CREATE', '', 'PAYMENT_METHODS', now(), '{systemUserId}'),
                ('PAYMENT_METHODS.DELETE', 'PAYMENT_METHODS.DELETE', '', 'PAYMENT_METHODS', now(), '{systemUserId}'),
                ('PAYMENT_METHODS.UPDATE', 'PAYMENT_METHODS.UPDATE', '', 'PAYMENT_METHODS', now(), '{systemUserId}'),
                ('PAYMENT_METHODS.VIEW', 'PAYMENT_METHODS.VIEW', '', 'PAYMENT_METHODS', now(), '{systemUserId}'),
                ('PERMISSIONS.CREATE', 'PERMISSIONS.CREATE', '', 'PERMISSIONS', now(), '{systemUserId}'),
                ('PERMISSIONS.DELETE', 'PERMISSIONS.DELETE', '', 'PERMISSIONS', now(), '{systemUserId}'),
                ('PERMISSIONS.UPDATE', 'PERMISSIONS.UPDATE', '', 'PERMISSIONS', now(), '{systemUserId}'),
                ('PERMISSIONS.VIEW', 'PERMISSIONS.VIEW', '', 'PERMISSIONS', now(), '{systemUserId}'),
                ('PRODUCTS.CREATE', 'PRODUCTS.CREATE', '', 'PRODUCTS', now(), '{systemUserId}'),
                ('PRODUCTS.DELETE', 'PRODUCTS.DELETE', '', 'PRODUCTS', now(), '{systemUserId}'),
                ('PRODUCTS.HARD_DELETE', 'PRODUCTS.HARD_DELETE', '', 'PRODUCTS', now(), '{systemUserId}'),
                ('PRODUCTS.READ', 'PRODUCTS.READ', '', 'PRODUCTS', now(), '{systemUserId}'),
                ('PRODUCTS.UPDATE', 'PRODUCTS.UPDATE', '', 'PRODUCTS', now(), '{systemUserId}'),
             ('BRANDS.CREATE', 'BRANDS.CREATE', '', 'BRANDS', now(), '{systemUserId}'),
             ('BRANDS.DELETE', 'BRANDS.DELETE', '', 'BRANDS', now(), '{systemUserId}'),
             ('BRANDS.HARD_DELETE', 'BRANDS.HARD_DELETE', '', 'BRANDS', now(), '{systemUserId}'),
             ('BRANDS.READ', 'BRANDS.READ', '', 'BRANDS', now(), '{systemUserId}'),
             ('BRANDS.UPDATE', 'BRANDS.UPDATE', '', 'BRANDS', now(), '{systemUserId}'),
                ('REFERRAL_REWARDS.CREATE', 'REFERRAL_REWARDS.CREATE', '', 'REFERRAL_REWARDS', now(), '{systemUserId}'),
                ('REFERRAL_REWARDS.DELETE', 'REFERRAL_REWARDS.DELETE', '', 'REFERRAL_REWARDS', now(), '{systemUserId}'),
                ('REFERRAL_REWARDS.UPDATE', 'REFERRAL_REWARDS.UPDATE', '', 'REFERRAL_REWARDS', now(), '{systemUserId}'),
                ('REFERRAL_REWARDS.VIEW', 'REFERRAL_REWARDS.VIEW', '', 'REFERRAL_REWARDS', now(), '{systemUserId}'),
                ('RETURNS.CREATE', 'RETURNS.CREATE', '', 'RETURNS', now(), '{systemUserId}'),
                ('RETURNS.DELETE', 'RETURNS.DELETE', '', 'RETURNS', now(), '{systemUserId}'),
                ('RETURNS.READ', 'RETURNS.READ', '', 'RETURNS', now(), '{systemUserId}'),
                ('RETURNS.UPDATE', 'RETURNS.UPDATE', '', 'RETURNS', now(), '{systemUserId}'),
                ('ROLES.CREATE', 'ROLES.CREATE', '', 'ROLES', now(), '{systemUserId}'),
                ('ROLES.DELETE', 'ROLES.DELETE', '', 'ROLES', now(), '{systemUserId}'),
                ('ROLES.PERMISSION_ADD', 'ROLES.PERMISSION_ADD', '', 'ROLES', now(), '{systemUserId}'),
                ('ROLES.UPDATE', 'ROLES.UPDATE', '', 'ROLES', now(), '{systemUserId}'),
                ('ROLES.VIEW', 'ROLES.VIEW', '', 'ROLES', now(), '{systemUserId}'),
                ('SETTINGS.MANAGE', 'SETTINGS.MANAGE', '', 'SETTINGS', now(), '{systemUserId}'),
                ('SETTINGS.OVERRIDE', 'SETTINGS.OVERRIDE', '', 'SETTINGS', now(), '{systemUserId}'),
                ('SHIPMENTS.ALL', 'SHIPMENTS.ALL', '', 'SHIPMENTS', now(), '{systemUserId}'),
                ('SHIPMENTS.CREATE', 'SHIPMENTS.CREATE', '', 'SHIPMENTS', now(), '{systemUserId}'),
                ('SHIPMENTS.DELETE', 'SHIPMENTS.DELETE', '', 'SHIPMENTS', now(), '{systemUserId}'),
                ('SHIPMENTS.UPDATE', 'SHIPMENTS.UPDATE', '', 'SHIPMENTS', now(), '{systemUserId}'),
                ('SHIPMENTS.VIEW', 'SHIPMENTS.VIEW', '', 'SHIPMENTS', now(), '{systemUserId}'),
                ('STOCKMOVEMENTS.CREATE', 'STOCKMOVEMENTS.CREATE', '', 'STOCKMOVEMENTS', now(), '{systemUserId}'),
                ('STOCKMOVEMENTS.DELETE', 'STOCKMOVEMENTS.DELETE', '', 'STOCKMOVEMENTS', now(), '{systemUserId}'),
                ('STOCKMOVEMENTS.UPDATE', 'STOCKMOVEMENTS.UPDATE', '', 'STOCKMOVEMENTS', now(), '{systemUserId}'),
                ('STOCKMOVEMENTS.VIEW', 'STOCKMOVEMENTS.VIEW', '', 'STOCKMOVEMENTS', now(), '{systemUserId}'),
                ('SUPPLIERS.DELETE', 'SUPPLIERS.DELETE', '', 'SUPPLIERS', now(), '{systemUserId}'),
                ('SUPPLIERS.EDIT', 'SUPPLIERS.EDIT', '', 'SUPPLIERS', now(), '{systemUserId}'),
                ('SUPPLIERS.VIEW', 'SUPPLIERS.VIEW', '', 'SUPPLIERS', now(), '{systemUserId}'),
                ('USERS.ROLE_CHANGE', 'USERS.ROLE_CHANGE', '', 'USERS', now(), '{systemUserId}'),
                ('USERS.SOFT_DELETE', 'USERS.SOFT_DELETE', '', 'USERS', now(), '{systemUserId}'),
                ('USERS.UPDATE', 'USERS.UPDATE', '', 'USERS', now(), '{systemUserId}'),
                ('VENDORS.ALL', 'VENDORS.ALL', '', 'VENDORS', now(), '{systemUserId}'),
                ('VENDORS.CREATE', 'VENDORS.CREATE', '', 'VENDORS', now(), '{systemUserId}'),
                ('VENDORS.DELETE', 'VENDORS.DELETE', '', 'VENDORS', now(), '{systemUserId}'),
                ('VENDORS.SOFT_DELETE', 'VENDORS.SOFT_DELETE', '', 'VENDORS', now(), '{systemUserId}'),
                ('VENDORS.UPDATE', 'VENDORS.UPDATE', '', 'VENDORS', now(), '{systemUserId}'),
                ('VENDORS.UPDATE_SELF', 'VENDORS.UPDATE_SELF', '', 'VENDORS', now(), '{systemUserId}'),
                ('VENDORS.VERIFY', 'VENDORS.VERIFY', '', 'VENDORS', now(), '{systemUserId}'),
                ('VENDORS.VIEW', 'VENDORS.VIEW', '', 'VENDORS', now(), '{systemUserId}'),
                ('VENDOR.KYC.SUBMIT', 'VENDOR.KYC.SUBMIT', '', 'VENDOR.KYC', now(), '{systemUserId}'),
                ('VENDOR.KYC.VIEW', 'VENDOR.KYC.VIEW', '', 'VENDOR.KYC', now(), '{systemUserId}'),
                ('VENDOR.KYC.REVIEW', 'VENDOR.KYC.REVIEW', '', 'VENDOR.KYC', now(), '{systemUserId}')
              ON CONFLICT (code) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // 6) TEST USERS
            using (var cmd = new NpgsqlCommand(@"INSERT INTO sys.users (id, username, first_name, last_name, email, phone, password_hash, kyc_status, is_email_verified)
              VALUES
              ('11111111-1111-1111-1111-111111111111','verifieduser','Verified','User','verified@example.com','5550000001','seedhash','VERIFIED', TRUE),
              ('22222222-2222-2222-2222-222222222222','pendinguser','Pending','User','pending@example.com','5550000002','seedhash','PENDING', TRUE),
              ('33333333-3333-3333-3333-333333333333','vendoruser','Vendor','User','vendor@example.com','5550000003','seedhash','VERIFIED', TRUE)
              ON CONFLICT (username) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // 7) KYC VERIFICATIONS
            using (var cmd = new NpgsqlCommand(@"INSERT INTO sys.kyc_verifications (id, user_id, national_id, country, document_type, document_number, selfie_url, document_front_url, document_back_url, status)
              VALUES
              ('aaaa1111-1111-1111-1111-aaaaaaaaaaaa','11111111-1111-1111-1111-111111111111','11111111111','TR','ID','A123','http://example.com/selfie1.jpg','http://example.com/front1.jpg','http://example.com/back1.jpg','VERIFIED'),
              ('bbbb2222-2222-2222-2222-bbbbbbbbbbbb','22222222-2222-2222-2222-222222222222','22222222222','TR','PASSPORT','B123','http://example.com/selfie2.jpg','http://example.com/front2.jpg','http://example.com/back2.jpg','PENDING')
              ON CONFLICT (id) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }
       
            // 8) ROLE-PERMISSIONS: Admin = tüm izinler
            using (var cmd = new NpgsqlCommand($@"INSERT INTO sys.role_permissions (role_id, permission_id, created_by, created_at)
                SELECT r.id, p.id, '{systemUserId}', now()
                  FROM sys.roles r CROSS JOIN sys.permissions p
                 WHERE r.name = 'SYSTEM_ADMIN'
                ON CONFLICT (role_id, permission_id) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // 9) ROLE-PERMISSIONS: Vendor = sınırlı izinler
            using (var cmd = new NpgsqlCommand($@"INSERT INTO sys.role_permissions (role_id, permission_id, created_by, created_at)
                SELECT r.id, p.id, '{systemUserId}', now()
                  FROM sys.roles r
                  JOIN sys.permissions p ON p.group_name IN ('PRODUCTS','ORDERS','SHIPMENTS','KYC','FILEUPLOADS','VENDORS')
                 WHERE r.name = 'VENDOR'
                ON CONFLICT (role_id, permission_id) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }
            // 9.1) ROLE-PERMISSIONS: Vendor KYC submit/view
            using (var cmd = new NpgsqlCommand($@"INSERT INTO sys.role_permissions (role_id, permission_id, created_by, created_at)
                SELECT r.id, p.id, '{systemUserId}', now()
                  FROM sys.roles r
                  JOIN sys.permissions p ON p.code IN ('VENDOR.KYC.SUBMIT','VENDOR.KYC.VIEW')
                 WHERE r.name = 'VENDOR'
                ON CONFLICT (role_id, permission_id) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // 9.2) ROLE-PERMISSIONS: Vendor KYC review
            using (var cmd = new NpgsqlCommand($@"INSERT INTO sys.role_permissions (role_id, permission_id, created_by, created_at)
                SELECT r.id, p.id, '{systemUserId}', now()
                  FROM sys.roles r
                  JOIN sys.permissions p ON p.code = 'VENDOR.KYC.REVIEW'
                 WHERE r.name = 'SUPPORT'
                ON CONFLICT (role_id, permission_id) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }


            // 10) USER-ROLES eşleştirme
            using (var cmd = new NpgsqlCommand($@"INSERT INTO sys.user_roles (user_id, role_id, created_at)
                SELECT u.id, r.id, now() FROM sys.users u CROSS JOIN sys.roles r
                 WHERE u.username = 'systemadmin' AND r.name = 'SYSTEM_ADMIN'
                ON CONFLICT (user_id, role_id) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new NpgsqlCommand(@"INSERT INTO sys.user_roles (user_id, role_id, created_at)
                SELECT u.id, r.id, now() FROM sys.users u JOIN sys.roles r ON r.name = 'USER'
                 WHERE u.username IN ('verifieduser','pendinguser')
                ON CONFLICT (user_id, role_id) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new NpgsqlCommand(@"INSERT INTO sys.user_roles (user_id, role_id, created_at)
                SELECT u.id, r.id, now() FROM sys.users u JOIN sys.roles r ON r.name = 'VENDOR'
                 WHERE u.username = 'vendoruser'
                ON CONFLICT (user_id, role_id) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // VENDORS
            using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO sys.vendors (id, name, email, phone, address, created_at)
                    VALUES
                     ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','Acme Inc','sales@acme.test','+90 555 111 11 11','İst.', NOW()),
                     ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb','Globex','info@globex.test','+90 555 222 22 22','Ank.', NOW())
                    ON CONFLICT (name) DO NOTHING;", connection, tx))
                                {
                                    cmd.ExecuteNonQuery();
                                }
            // 7.1) VENDOR KYC VERIFICATIONS
            using (var cmd = new NpgsqlCommand(@"INSERT INTO sys.vendor_kyc_verifications (id, vendor_id, tax_id, country, document_type, document_number, document_url, status)
              VALUES
              ('cccc3333-3333-3333-3333-cccccccccccc','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','1234567890','TR','ID','TAX123','http://example.com/vendor-doc.jpg','PENDING')
              ON CONFLICT (vendor_id) DO NOTHING;", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }


            // CATEGORIES (electronics parent, phone child)
            using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO sys.product_categories (id, name, description, created_at)
                    VALUES 
                     ('cccccccc-cccc-cccc-cccc-cccccccccccc','Electronics','Electronics root', NOW())
                    ON CONFLICT (name) DO NOTHING;", connection, tx))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO sys.product_categories (id, name, parent_category_id, description, created_at)
                    VALUES 
                     ('dddddddd-dddd-dddd-dddd-dddddddddddd','Smartphones','cccccccc-cccc-cccc-cccc-cccccccccccc','Phones', NOW())
                    ON CONFLICT (name) DO NOTHING;", connection, tx))
                                {
                                    cmd.ExecuteNonQuery();
                                }
            // === BRANDS ===
                     using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO sys.brands (id, name, slug, website, logo_url, status, created_at, created_by)
                    VALUES
                      ('b0000000-0000-0000-0000-000000000001', 'Acme', 'acme', 'https://www.acme.com', NULL, 'ACTIVE', NOW(), @created_by),
                      ('b0000000-0000-0000-0000-000000000002', 'Globex', 'globex', 'https://www.globex.com', NULL, 'ACTIVE', NOW(), @created_by),
                      ('b0000000-0000-0000-0000-000000000003', 'Initech', 'initech', 'https://www.initech.com', NULL, 'ACTIVE', NOW(), @created_by)
                    ON CONFLICT (name) DO NOTHING;
                ", connection, tx))
                            {
                                cmd.Parameters.AddWithValue("@created_by", Guid.Parse(systemUserId));
                                cmd.ExecuteNonQuery();
                            }


            using (var cmd = new NpgsqlCommand(@"
    INSERT INTO sys.brand_translations (brand_id, language_code, name, description)
    SELECT id, 'tr', name, name || ' markası' FROM sys.brands
    WHERE name IN ('Acme', 'Globex', 'Initech')
    ON CONFLICT (brand_id, language_code) DO NOTHING;
", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // PRODUCTS (brand_id eklendi, 6 ürüne çoğaltıldı)
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.products
(id, vendor_id, category_id, brand_id, product_type, price, unit, barcode, stock_quantity, is_stock_tracked, sku, created_at)
VALUES
  -- Acme
  ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','dddddddd-dddd-dddd-dddd-dddddddddddd',
   (SELECT id FROM sys.brands WHERE name='Acme'), 'PHYSICAL',14999,'piece','ACME-PHONE-1',100,TRUE,'ACM-IPH-001',NOW()),
  ('aaaaeeee-eeee-eeee-eeee-eeeeeeee0001','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','dddddddd-dddd-dddd-dddd-dddddddddddd',
   (SELECT id FROM sys.brands WHERE name='Acme'), 'PHYSICAL',16999,'piece','ACME-PHONE-2',120,TRUE,'ACM-IPH-002',NOW()),

  -- Globex
  ('ffffffff-ffff-ffff-ffff-ffffffffffff','bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb','dddddddd-dddd-dddd-dddd-dddddddddddd',
   (SELECT id FROM sys.brands WHERE name='Globex'), 'PHYSICAL', 8999,'piece','GLOBEX-PHONE-1',250,TRUE,'GLX-AND-001',NOW()),
  ('bbbfffff-ffff-ffff-ffff-ffffffff0002','bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb','dddddddd-dddd-dddd-dddd-dddddddddddd',
   (SELECT id FROM sys.brands WHERE name='Globex'), 'PHYSICAL',11999,'piece','GLOBEX-PHONE-2',180,TRUE,'GLX-AND-002',NOW()),

  -- Initech
  ('cccc1111-2222-3333-4444-555555555555','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','dddddddd-dddd-dddd-dddd-dddddddddddd',
   (SELECT id FROM sys.brands WHERE name='Initech'), 'PHYSICAL',3999,'piece','INITECH-WATCH-1',500,TRUE,'INT-WTC-001',NOW()),
  ('dddd1111-2222-3333-4444-666666666666','bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb','dddddddd-dddd-dddd-dddd-dddddddddddd',
   (SELECT id FROM sys.brands WHERE name='Initech'), 'PHYSICAL',1299,'piece','INITECH-BUDS-1',800,TRUE,'INT-BUD-001',NOW())
ON CONFLICT (sku) DO UPDATE
SET brand_id = EXCLUDED.brand_id;
", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // PRODUCT TRANSLATIONS (TR + EN, genişletildi)
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.product_translations (id, product_id, language_code, name, description)
VALUES
  -- ACM-IPH-001
  ('11111111-2222-3333-4444-555555555555','eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee','tr','Acme Phone 1','Amiral gemisi model'),
  ('22222222-3333-4444-5555-666666666666','eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee','en','Acme Phone 1','Flagship model'),

  -- GLX-AND-001
  ('33333333-4444-5555-6666-777777777777','ffffffff-ffff-ffff-ffff-ffffffffffff','tr','Globex Android 1','Fiyat/performans'),
  ('44444444-5555-6666-7777-888888888888','ffffffff-ffff-ffff-ffff-ffffffffffff','en','Globex Android 1','Value for money'),

  -- ACM-IPH-002
  ('55555555-6666-7777-8888-999999999999','aaaaeeee-eeee-eeee-eeee-eeeeeeee0001','tr','Acme Phone 2','Daha güçlü işlemci ve kamera'),
  ('66666666-7777-8888-9999-000000000000','aaaaeeee-eeee-eeee-eeee-eeeeeeee0001','en','Acme Phone 2','Faster processor and camera'),

  -- GLX-AND-002
  ('77777777-8888-9999-0000-111111111111','bbbfffff-ffff-ffff-ffff-ffffffff0002','tr','Globex Android 2','Geniş batarya, hızlı şarj'),
  ('88888888-9999-0000-1111-222222222222','bbbfffff-ffff-ffff-ffff-ffffffff0002','en','Globex Android 2','Large battery, fast charging'),

  -- INT-WTC-001
  ('99999999-0000-1111-2222-333333333333','cccc1111-2222-3333-4444-555555555555','tr','Initech Watch','Akıllı saat, sağlık sensörleri'),
  ('00000000-1111-2222-3333-444444444444','cccc1111-2222-3333-4444-555555555555','en','Initech Watch','Smart watch with health sensors'),

  -- INT-BUD-001
  ('aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee','dddd1111-2222-3333-4444-666666666666','tr','Initech Buds','Kablosuz kulaklık, gürültü engelleme'),
  ('bbbbbbbb-cccc-dddd-eeee-ffffffffffff','dddd1111-2222-3333-4444-666666666666','en','Initech Buds','Wireless earbuds with noise cancelling')
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }


            // PAYMENT METHODS
            using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO sys.payment_methods (id, name, description)
                    VALUES
                     ('99999999-9999-9999-9999-999999999991','Credit Card','Online POS'),
                     ('99999999-9999-9999-9999-999999999992','Wire Transfer','Havale/EFT'),
                     ('99999999-9999-9999-9999-999999999993','Cash On Delivery','Kapıda ödeme')
                    ON CONFLICT (name) DO NOTHING;", connection, tx))
                                {
                                    cmd.ExecuteNonQuery();
                                }

                                // DISCOUNTS (enum’a uygun: PERCENTAGE, FIXED, SHIPPING ve CART/PRODUCT/CATEGORY/ALL_PRODUCTS)
                                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO sys.discounts
                    (code, discount_type, discount_value, target_type, target_value, min_cart_total, usage_limit, used_count, is_single_use_per_user,
                     valid_from, valid_until, is_active, created_by)
                    VALUES
                     -- Sepet geneli %10 (ALL_PRODUCTS yerine CART kullanmak istersen CART + NULL da olur)
                     ('SUMMER10', 'PERCENTAGE', 10, 'ALL_PRODUCTS', NULL, NULL, 100, 0, FALSE,
                      NOW(), NOW() + INTERVAL '30 days', TRUE, @created_by),

                     -- Belirli kategori %20 (Smartphones)
                     ('CAT20', 'PERCENTAGE', 20, 'CATEGORY', 'dddddddd-dddd-dddd-dddd-dddddddddddd', NULL, 200, 0, FALSE,
                      NOW(), NOW() + INTERVAL '15 days', TRUE, @created_by),

                     -- Sabit 50₺ indirim, min 500₺ sepet
                     ('SAVE50', 'FIXED', 50, 'CART', NULL, 500, 50, 0, FALSE,
                      NOW(), NOW() + INTERVAL '20 days', TRUE, @created_by),

                     -- Ücretsiz kargo kuponu (iş mantığın destekliyorsa)
                     ('FREESHIP', 'SHIPPING', 0, 'ALL_PRODUCTS', NULL, 100, 500, 0, TRUE,
                      NOW(), NOW() + INTERVAL '60 days', TRUE, @created_by)

                    ON CONFLICT (code) DO UPDATE
                    SET discount_type = EXCLUDED.discount_type,
                        discount_value = EXCLUDED.discount_value,
                        target_type = EXCLUDED.target_type,
                        target_value = EXCLUDED.target_value,
                        min_cart_total = EXCLUDED.min_cart_total,
                        usage_limit = EXCLUDED.usage_limit,
                        is_single_use_per_user = EXCLUDED.is_single_use_per_user,
                        valid_from = EXCLUDED.valid_from,
                        valid_until = EXCLUDED.valid_until,
                        is_active = EXCLUDED.is_active,
                        updated_at = NOW(),
                        updated_by = EXCLUDED.created_by;", connection, tx))
                                {
                                    cmd.Parameters.AddWithValue("@created_by", Guid.Parse("00000000-0000-0000-0000-000000000001"));
                                    cmd.ExecuteNonQuery();
                                }

            // BUYER + ADDRESS (verifieduser'a bağlı)
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.buyers (id, buyer_type, name, email, phone, linked_user_id, created_at)
VALUES ('12121212-1212-1212-1212-121212121212','INDIVIDUAL','Verified User','verified@example.com','5550000001',
        '11111111-1111-1111-1111-111111111111', NOW())
ON CONFLICT (email) DO NOTHING;

INSERT INTO sys.buyer_addresses (id, buyer_id, address_title, full_address, city, postal_code, country, is_default, created_at)
VALUES ('13131313-1313-1313-1313-131313131313','12121212-1212-1212-1212-121212121212',
        'Home','Mah. Cad. No:1','İstanbul','34000','TR', TRUE, NOW())
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // STOK GİRİŞ (initial load)
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.stock_movements (id, product_id, supplier_id, movement_type, quantity, description, status, created_at, created_by)
VALUES
('aaaa0000-0000-0000-0000-aaaabbbb0001','eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', NULL, 'IN', 100, 'Initial load', 'ACTIVE', NOW(), @created_by),
('aaaa0000-0000-0000-0000-aaaabbbb0002','ffffffff-ffff-ffff-ffff-ffffffffffff', NULL, 'IN', 250, 'Initial load', 'ACTIVE', NOW(), @created_by)
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.Parameters.AddWithValue("@created_by", Guid.Parse(systemUserId));
                cmd.ExecuteNonQuery();
            }

            // CART + ITEMS (verifieduser)
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.carts (id, user_id, cart_status, created_at)
VALUES ('14141414-1414-1414-1414-141414141414','11111111-1111-1111-1111-111111111111','ACTIVE', NOW())
ON CONFLICT DO NOTHING;

INSERT INTO sys.cart_items (id, cart_id, product_id, quantity, unit_price, currency, created_at)
VALUES
('15151515-1515-1515-1515-151515151515','14141414-1414-1414-1414-141414141414','eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',1,14999,'TRY',NOW()),
('16161616-1616-1616-1616-161616161616','14141414-1414-1414-1414-141414141414','ffffffff-ffff-ffff-ffff-ffffffffffff',2, 8999,'TRY',NOW())
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }

            // ORDER + DETAILS + DISCOUNT USAGE + STOKTAN DÜŞME
            using (var cmd = new NpgsqlCommand(@"
-- Order (SUMMER10 %10 uygulandı)
INSERT INTO sys.orders
(id, vendor_id, buyer_id, buyer_description, total_amount, currency, order_status, discount_code, discount_amount,
 payment_status, order_date, created_at, created_by)
VALUES
('17171717-1717-1717-1717-171717171717',
 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
 '12121212-1212-1212-1212-121212121212',
 'Verified User / Fatura',
 (14999 + 8999*2) * 0.90, 'TRY',
 'PAID', 'SUMMER10', (14999 + 8999*2) * 0.10,
 'PAID', NOW(), NOW(), @created_by)
ON CONFLICT DO NOTHING;

-- Order details
INSERT INTO sys.orders_details
(id, order_id, product_id, invoice_address_id, quantity, unit_price, discount, note, created_at, created_by)
VALUES
('18181818-1818-1818-1818-181818181818','17171717-1717-1717-1717-171717171717','eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
 '13131313-1313-1313-1313-131313131313',1,14999,0,'Acme Phone',NOW(),@created_by),
('19191919-1919-1919-1919-191919191919','17171717-1717-1717-1717-171717171717','ffffffff-ffff-ffff-ffff-ffffffffffff',
 '13131313-1313-1313-1313-131313131313',2, 8999,0,'Globex Android',NOW(),@created_by)
ON CONFLICT DO NOTHING;

-- Discount usage
INSERT INTO sys.discount_usages (id, discount_id, user_id, order_id, used_at)
VALUES (
  '1a1a1a1a-1a1a-1a1a-1a1a-1a1a1a1a1a1a',
  (SELECT id FROM sys.discounts WHERE code='SUMMER10'),
  '11111111-1111-1111-1111-111111111111',
  '17171717-1717-1717-1717-171717171717',
  NOW()
)
ON CONFLICT DO NOTHING;

-- Stock OUT
INSERT INTO sys.stock_movements (id, product_id, movement_type, quantity, description, related_order_id, status, created_at, created_by)
VALUES
('aaaa0000-0000-0000-0000-aaaabbbb1001','eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee','OUT',1,'Order shipment',
 '17171717-1717-1717-1717-171717171717','ACTIVE',NOW(),@created_by),
('aaaa0000-0000-0000-0000-aaaabbbb1002','ffffffff-ffff-ffff-ffff-ffffffffffff','OUT',2,'Order shipment',
 '17171717-1717-1717-1717-171717171717','ACTIVE',NOW(),@created_by)
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.Parameters.AddWithValue("@created_by", Guid.Parse(systemUserId));
                cmd.ExecuteNonQuery();
            }

            // ÖDEME (Credit Card -> CONFIRMED)
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.payments (id, order_id, payment_method_id, payment_status, transaction_hash, paid_at, created_at, created_by)
VALUES ('20202020-2020-2020-2020-202020202020','17171717-1717-1717-1717-171717171717',
        '99999999-9999-9999-9999-999999999991','CONFIRMED','0xHASH123',NOW(),NOW(),@created_by)
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.Parameters.AddWithValue("@created_by", Guid.Parse(systemUserId));
                cmd.ExecuteNonQuery();
            }

            // KARGO + ORDER STATUS LOG
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.shipments (id, order_id, address_id, tracking_number, shipping_provider, shipment_status, shipped_at, created_at, created_by)
VALUES ('21212121-2121-2121-2121-212121212121','17171717-1717-1717-1717-171717171717',
        '13131313-1313-1313-1313-131313131313','ACME-TRK-1001','Yurtiçi','SHIPPED',NOW(),NOW(),@created_by)
ON CONFLICT DO NOTHING;

INSERT INTO sys.order_status_log (id, order_id, old_status, new_status, notes, changed_at, changed_by)
VALUES
('22222222-aaaa-bbbb-cccc-333333333333','17171717-1717-1717-1717-171717171717',NULL,'PENDING','Created',NOW(),@created_by),
('22222222-aaaa-bbbb-cccc-444444444444','17171717-1717-1717-1717-171717171717','PENDING','PAID','Payment confirmed',NOW(),@created_by),
('22222222-aaaa-bbbb-cccc-555555555555','17171717-1717-1717-1717-171717171717','PAID','SHIPPED','Shipped',NOW(),@created_by)
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.Parameters.AddWithValue("@created_by", Guid.Parse(systemUserId));
                cmd.ExecuteNonQuery();
            }

            // FATURA
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.invoices (id, order_id, invoice_number, issued_at, total_amount, currency, buyer_address_id, pdf_url, created_at, created_by)
VALUES ('23232323-2323-2323-2323-232323232323','17171717-1717-1717-1717-171717171717','INV-2025-0001',NOW(),
        (14999 + 8999*2) * 0.90,'TRY','13131313-1313-1313-1313-131313131313',
        'http://example.com/invoices/INV-2025-0001.pdf',NOW(),@created_by)
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.Parameters.AddWithValue("@created_by", Guid.Parse(systemUserId));
                cmd.ExecuteNonQuery();
            }

            // VENDOR SETTINGS
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.vendor_settings (id, vendor_id, key, value, created_at, created_by)
VALUES
('24242424-2424-2424-2424-242424242424','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','ORDER_AUTO_ACCEPT','true',NOW(),@created_by),
('25252525-2525-2525-2525-252525252525','bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb','DEFAULT_SHIPPING_PROVIDER','Yurtiçi',NOW(),@created_by)
ON CONFLICT (vendor_id, key) DO NOTHING;
", connection, tx))
            {
                cmd.Parameters.AddWithValue("@created_by", Guid.Parse(systemUserId));
                cmd.ExecuteNonQuery();
            }

            // REFERRAL REWARD (verifieduser -> pendinguser)
            using (var cmd = new NpgsqlCommand(@"
INSERT INTO sys.referral_rewards
(id, referred_user_id, referrer_user_id, reward_type, reward_amount, reward_currency, reward_description,
 reward_status, triggered_event, is_reward_granted, reward_expiration_date, created_at, created_by)
VALUES ('26262626-2626-2626-2626-262626262626',
        '22222222-2222-2222-2222-222222222222',  -- pendinguser
        '11111111-1111-1111-1111-111111111111',  -- verifieduser
        'DISCOUNT', 25, 'TRY', 'Welcome bonus for referral',
        'GRANTED', 'REGISTRATION', TRUE, NOW() + INTERVAL '30 days', NOW(), @created_by)
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.Parameters.AddWithValue("@created_by", Guid.Parse(systemUserId));
                cmd.ExecuteNonQuery();
            }

            // Seed demo brand, category, product and image
            using (var cmd = new NpgsqlCommand(@"
 

INSERT INTO sys.product_categories (id, name, slug, created_at)
VALUES ('bbbbbbbb-0000-0000-0000-bbbbbbbb0000','Phones','phones', now())
ON CONFLICT (id) DO NOTHING;

INSERT INTO sys.products (id, brand_id, category_id, slug, price, status, created_at)
VALUES (
  'cccccccc-0000-0000-0000-cccccccc0000',
  (SELECT id FROM sys.brands WHERE name='Acme'),
  'bbbbbbbb-0000-0000-0000-bbbbbbbb0000',
  'acme-phone-1', 199.99,'ACTIVE', now()
)
ON CONFLICT (id) DO NOTHING;

INSERT INTO sys.product_translations (id, product_id, language_code, name, description)
VALUES ('dddddddd-0000-0000-0000-dddddddd0000','cccccccc-0000-0000-0000-cccccccc0000','en','Acme Phone','Demo product')
ON CONFLICT (product_id, language_code) DO NOTHING;

INSERT INTO sys.product_images (id, product_id, image_url, alt_text, sort_order, is_primary)
VALUES ('eeeeeeee-0000-0000-0000-eeeeeeee0000','cccccccc-0000-0000-0000-cccccccc0000','https://example.com/acme-phone.jpg','Acme Phone',0,TRUE)
ON CONFLICT DO NOTHING;
", connection, tx))
            {
                cmd.ExecuteNonQuery();
            }


            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
