using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Sky.Template.Backend.Core.Initializer;

public static class DatabaseInitializer
{
    public static void InitializeDatabase(IConfiguration configuration)
    {
        var provider = configuration["DatabaseConnection:Provider"];
        if (provider != "PostgreSql") return;

        var connectionString = configuration["DatabaseConnection:ConnectionString"];
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var sql = @"
                    CREATE EXTENSION IF NOT EXISTS ""pgcrypto"";

                    DO $$
                    BEGIN
                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'status_enum') THEN
                        CREATE TYPE status_enum AS ENUM ('ACTIVE', 'INACTIVE', 'SUSPENDED', 'DELETED');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'kyc_status_enum') THEN
                        CREATE TYPE kyc_status_enum AS ENUM ('UNVERIFIED', 'PENDING', 'VERIFIED', 'REJECTED', 'EXPIRED');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'document_type_enum') THEN
                        CREATE TYPE document_type_enum AS ENUM ('ID', 'PASSPORT', 'DRIVER_LICENSE');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'buyer_type_enum') THEN
                        CREATE TYPE buyer_type_enum AS ENUM ('INDIVIDUAL', 'CORPORATE');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'product_type_enum') THEN
                        CREATE TYPE product_type_enum AS ENUM ('PHYSICAL', 'DIGITAL', 'SERVICE');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'order_status_enum') THEN
                        CREATE TYPE order_status_enum AS ENUM ('PENDING', 'PAID', 'SHIPPED', 'DELIVERED', 'CANCELLED', 'FAILED');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'payment_status_enum') THEN
                        CREATE TYPE payment_status_enum AS ENUM ('AWAITING', 'PAID', 'CONFIRMED', 'FAILED', 'REFUNDED');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'supplier_type_enum') THEN
                        CREATE TYPE supplier_type_enum AS ENUM ('REGULAR', 'PREMIUM', 'EXCLUSIVE');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'shipment_status_enum') THEN
                        CREATE TYPE shipment_status_enum AS ENUM ('PREPARING', 'SHIPPED', 'DELIVERED', 'CANCELLED', 'RETURNED');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'discount_type_enum') THEN
                            CREATE TYPE discount_type_enum AS ENUM ('PERCENTAGE', 'FIXED', 'SHIPPING');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'discount_target_enum') THEN
                            CREATE TYPE discount_target_enum AS ENUM ('CART', 'PRODUCT', 'CATEGORY', 'ALL_PRODUCTS');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'cart_status_enum') THEN
                        CREATE TYPE cart_status_enum AS ENUM ('ACTIVE', 'ABANDONED', 'CONVERTED');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'return_status_enum') THEN
                        CREATE TYPE return_status_enum AS ENUM ('PENDING', 'APPROVED', 'REJECTED');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'permission_action_enum') THEN
                        CREATE TYPE permission_action_enum AS ENUM ('VIEW', 'CREATE', 'EDIT', 'DELETE', 'EXPORT', 'REPORT', 'SOFT_DELETE', 'ROLE_ASSIGN');
                      END IF;

                      IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'movement_type_enum') THEN
                        CREATE TYPE movement_type_enum AS ENUM ('IN', 'OUT', 'RETURN', 'CORRECTION');
                      END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'referral_reward_status_enum') THEN
                      CREATE TYPE referral_reward_status_enum AS ENUM ('PENDING', 'GRANTED', 'EXPIRED', 'REJECTED');
                    END IF;

                    END
                    $$;


                    CREATE SCHEMA IF NOT EXISTS sys;

                    CREATE TABLE IF NOT EXISTS sys.users (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    username VARCHAR(50) UNIQUE NOT NULL,
                    first_name VARCHAR(100) NOT NULL,
                    last_name VARCHAR(100) NOT NULL,
                    email VARCHAR(100) UNIQUE NOT NULL,
                    image_path varchar(500),
                    phone VARCHAR(15) UNIQUE NOT NULL,
                    vendor_id UUID, -- FK sonradan eklenecek

                    password_hash VARCHAR(200) NOT NULL,
                    kyc_status kyc_status_enum DEFAULT 'UNVERIFIED',
                    is_email_verified BOOLEAN DEFAULT FALSE,
                    preferred_language VARCHAR(10),  -- 'en', 'tr' gibi
                    status status_enum DEFAULT 'ACTIVE',
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    last_login_date TIMESTAMP,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    referred_by UUID REFERENCES sys.users(id) ON DELETE RESTRICT,

                    delete_reason TEXT
                    );

                    CREATE TABLE IF NOT EXISTS sys.roles (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(50) UNIQUE NOT NULL,
                    description TEXT,
                    status status_enum DEFAULT 'ACTIVE',
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    deleted_at TIMESTAMP,
                    delete_reason TEXT
                    );

                    -- Product Categories
                    CREATE TABLE IF NOT EXISTS sys.product_categories (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    name VARCHAR(100) UNIQUE NOT NULL,
                    slug VARCHAR(300) UNIQUE,
                    parent_category_id UUID REFERENCES sys.product_categories(id) NULL,
                    description TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );

                    CREATE TABLE IF NOT EXISTS sys.resources (
                    id SERIAL PRIMARY KEY,
                    code VARCHAR(100) UNIQUE NOT NULL,     -- Örn: 'PRODUCTS', 'USERS'
                    name VARCHAR(100) NOT NULL,            -- Varsayılan gösterim adı
                    description TEXT,
                    status status_enum DEFAULT 'ACTIVE',
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    deleted_at TIMESTAMP,
                    delete_reason TEXT
                    );



                    CREATE TABLE IF NOT EXISTS sys.vendors (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    name VARCHAR(255) NOT NULL UNIQUE,
                    slug TEXT UNIQUE NOT NULL,
                    short_description TEXT,
                    logo_url TEXT,
                    banner_url TEXT,
                    email VARCHAR(255),
                    phone VARCHAR(50),
                    address TEXT,
                    rating_avg NUMERIC(3,2) DEFAULT 0.00,
                    rating_count INT DEFAULT 0,
                    status status_enum DEFAULT 'ACTIVE',
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP
                    );

                    DO $$
                    BEGIN
                      IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_name = 'fk_users_vendor'
                        AND table_name = 'users'
                        AND constraint_type = 'FOREIGN KEY'
                      ) THEN
                        ALTER TABLE sys.users
                        ADD CONSTRAINT fk_users_vendor
                        FOREIGN KEY (vendor_id) REFERENCES sys.vendors(id) ON DELETE RESTRICT;
                      END IF;
                    END;
                    $$;

                    CREATE INDEX IF NOT EXISTS idx_vendors_status ON sys.vendors(status);
                    CREATE INDEX IF NOT EXISTS idx_vendors_slug ON sys.vendors (lower(slug));




                    CREATE TABLE IF NOT EXISTS sys.file_uploads (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

                    uploaded_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,

                    file_url VARCHAR(500) NOT NULL,            -- Dosyanın bulunduğu URL
                    file_type VARCHAR(50) NOT NULL,            -- MIME tipi: image/png, application/pdf vs.
                    file_name VARCHAR(255),                    -- Dosyanın orijinal adı (örnek: kimlik.pdf)
                    file_extension VARCHAR(20),                -- .jpg, .png, .pdf gibi
                    file_size BIGINT,                          -- Byte cinsinden dosya boyutu (örnek: 102400 → 100 KB)
                    storage_provider VARCHAR(50),           -- 'AWS', 'Azure', 'Local', 'IPFS' gibi
                    metadata JSONB,                         -- Örneğin EXIF verileri, thumbnail bilgisi gibi detaylar

                    context VARCHAR(100) NOT NULL,             -- Nerede kullanılıyor: 'KYC_DOCUMENT', 'INVOICE_PDF', vs.

                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );


                    CREATE TABLE IF NOT EXISTS sys.kyc_verifications (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    user_id UUID NOT NULL REFERENCES sys.users(id) ON DELETE CASCADE,

                    -- KYC bilgileri
                    national_id VARCHAR(20),
                    country VARCHAR(100),
                    document_type document_type_enum,          -- ENUM: 'ID', 'PASSPORT', 'DRIVER_LICENSE'
                    document_number VARCHAR(100),
                    document_expiry_date DATE,

                    -- Dosya URL'leri (isteğe göre dosya_id’ye çevrilebilir)
                    selfie_url VARCHAR(500),
                    document_front_url VARCHAR(500),
                    document_back_url VARCHAR(500),

                    -- Doğrulama durumu
                    status kyc_status_enum DEFAULT 'PENDING',  -- ENUM: 'PENDING', 'APPROVED', 'REJECTED'
                    reason TEXT,

                    -- Denetim
                    reviewed_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    reviewed_at TIMESTAMP,

                    -- Zaman damgaları
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,

                    -- Her kullanıcıya ait yalnızca 1 aktif başvuru olmalı
                    CONSTRAINT uq_kyc_user UNIQUE (user_id)
                    );
                    CREATE TABLE IF NOT EXISTS sys.vendor_kyc_verifications (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    vendor_id UUID NOT NULL REFERENCES sys.vendors(id) ON DELETE CASCADE,

                    tax_id VARCHAR(50),
                    country VARCHAR(100),
                    document_type document_type_enum,
                    document_number VARCHAR(100),
                    document_url VARCHAR(500),

                    status kyc_status_enum DEFAULT 'PENDING',
                    reason TEXT,

                    reviewed_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    reviewed_at TIMESTAMP,

                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL
                    );
                    CREATE UNIQUE INDEX IF NOT EXISTS uq_vendor_kyc_vendor ON sys.vendor_kyc_verifications(vendor_id);






                    CREATE TABLE IF NOT EXISTS sys.permissions (
                    id SERIAL PRIMARY KEY,

                    code VARCHAR(100) UNIQUE NOT NULL,          -- Sistemsel sabit değer: 'USER_CREATE', 'PRODUCT_VIEW' gibi
                    group_name VARCHAR(100),
                    name VARCHAR(100),                          -- Panelde gösterilecek isim (örn: 'Kullanıcı Oluştur')
                    description TEXT,                           -- Geliştirici/teknik açıklama
                    resource_id INT REFERENCES sys.resources(id) ON DELETE SET NULL,
                    action permission_action_enum DEFAULT 'VIEW',


                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,

                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );


                    CREATE TABLE IF NOT EXISTS sys.user_roles (
                    user_id UUID NOT NULL REFERENCES sys.users(id) ON DELETE CASCADE,
                    role_id INT NOT NULL REFERENCES sys.roles(id) ON DELETE RESTRICT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID,
                    updated_at TIMESTAMP,
                    updated_by UUID,
                    PRIMARY KEY (user_id, role_id)
                    );

                    CREATE TABLE IF NOT EXISTS sys.role_permissions (
                    role_id INT NOT NULL REFERENCES sys.roles(id) ON DELETE CASCADE,
                    permission_id INT NOT NULL REFERENCES sys.permissions(id) ON DELETE CASCADE,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID,
                    updated_at TIMESTAMP,
                    updated_by UUID,
                    PRIMARY KEY (role_id, permission_id)
                    );

                    CREATE TABLE IF NOT EXISTS sys.system_settings (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    key VARCHAR(100) UNIQUE NOT NULL,
                    group_name VARCHAR(100) NOT NULL,
                    value TEXT NOT NULL,
                    description TEXT,
                    is_public BOOLEAN DEFAULT FALSE,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );

                    CREATE TABLE IF NOT EXISTS sys.vendor_settings (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    vendor_id UUID NOT NULL REFERENCES sys.vendors(id) ON DELETE CASCADE,
                    key VARCHAR(100) NOT NULL,
                    value TEXT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT,
                    UNIQUE (vendor_id, key)
                    );

                    CREATE TABLE IF NOT EXISTS sys.audit_logs (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    user_id UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    action VARCHAR(100) NOT NULL,
                    table_name VARCHAR(100),
                    record_id VARCHAR(100),
                    old_values JSONB,
                    new_values JSONB,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS sys.error_logs (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    message TEXT NOT NULL,
                    stack_trace TEXT,
                    source VARCHAR(100),
                    path VARCHAR(200),
                    method VARCHAR(10),
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS sys.refresh_tokens (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    token VARCHAR(500) NOT NULL,
                    user_id UUID NOT NULL REFERENCES sys.users(id) ON DELETE CASCADE,
                    expiration_date TIMESTAMP NOT NULL,
                    schema_name VARCHAR(100),
                    created_by_ip VARCHAR(100),
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    revoked_at TIMESTAMP,
                    revoked_by_ip VARCHAR(100)
                    );

                        -- === BRANDS ===
                        CREATE TABLE IF NOT EXISTS sys.brands (
                          id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                          name VARCHAR(150) NOT NULL UNIQUE,
                          slug VARCHAR(180) UNIQUE,
                          website VARCHAR(255),
                          logo_url VARCHAR(500),
                          status status_enum DEFAULT 'ACTIVE',
                          created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                          created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                          updated_at TIMESTAMP,
                          updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                          is_deleted BOOLEAN DEFAULT FALSE,
                          deleted_at TIMESTAMP,
                          deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                          delete_reason TEXT
                        );

                        -- Çok dillilik istersen (opsiyonel ama tavsiye):
                        CREATE TABLE IF NOT EXISTS sys.brand_translations (
                          id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                          brand_id UUID NOT NULL REFERENCES sys.brands(id) ON DELETE CASCADE,
                          language_code VARCHAR(10) NOT NULL,  -- 'en', 'tr' gibi
                          name VARCHAR(200) NOT NULL,
                          description TEXT,
                          CONSTRAINT uq_brand_lang UNIQUE (brand_id, language_code)
                        );

                  
                    -- Products
                    CREATE TABLE IF NOT EXISTS sys.products (
                      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                      vendor_id UUID REFERENCES sys.vendors(id) ON DELETE SET NULL,
                      category_id UUID REFERENCES sys.product_categories(id) ON DELETE RESTRICT,
                      brand_id UUID REFERENCES sys.brands(id) ON DELETE SET NULL, -- eklendi
                      slug VARCHAR(300),
                      product_type product_type_enum,
                      price DECIMAL(12,2) NOT NULL,
                      unit VARCHAR(50), -- örn: 'piece', 'kg', 'liter'
                      barcode VARCHAR(100) UNIQUE,
                      stock_quantity DECIMAL(10,3),
                      is_stock_tracked BOOLEAN DEFAULT FALSE,
                      sku VARCHAR(100) UNIQUE,
                      status status_enum DEFAULT 'ACTIVE',
                      is_decimal_quantity_allowed BOOLEAN NOT NULL DEFAULT FALSE,
                      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                      created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                      updated_at TIMESTAMP,
                      updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                      is_deleted BOOLEAN DEFAULT FALSE,
                      deleted_at TIMESTAMP,
                      deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                      delete_reason TEXT
                    );


            
                    CREATE TABLE IF NOT EXISTS sys.product_translations (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    product_id UUID NOT NULL REFERENCES sys.products(id) ON DELETE CASCADE,
                    language_code VARCHAR(10) NOT NULL, -- 'en', 'tr', 'de' gibi
                    name VARCHAR(200) NOT NULL,
                    description TEXT,
                    CONSTRAINT uq_product_lang UNIQUE (product_id, language_code)
                    );

                    -- Add slugs
                    ALTER TABLE sys.product_categories ADD COLUMN IF NOT EXISTS slug VARCHAR(180);
                    CREATE UNIQUE INDEX IF NOT EXISTS uq_product_categories_slug ON sys.product_categories(slug) WHERE is_deleted = FALSE;

                    ALTER TABLE sys.products ADD COLUMN IF NOT EXISTS slug VARCHAR(180);
                    CREATE UNIQUE INDEX IF NOT EXISTS uq_products_slug ON sys.products(slug) WHERE is_deleted = FALSE;

                    -- Product images
                    CREATE TABLE IF NOT EXISTS sys.product_images (
                      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                      product_id UUID NOT NULL REFERENCES sys.products(id) ON DELETE CASCADE,
                      image_url VARCHAR(500) NOT NULL,
                      alt_text VARCHAR(200),
                      sort_order INT DEFAULT 0,
                      is_primary BOOLEAN DEFAULT FALSE,
                      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );
                    CREATE INDEX IF NOT EXISTS idx_product_images_product ON sys.product_images(product_id);
                    CREATE INDEX IF NOT EXISTS idx_product_images_primary ON sys.product_images(product_id, is_primary);

                    -- Attributes & Variants
                    CREATE TABLE IF NOT EXISTS sys.product_attributes (
                      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                      name VARCHAR(100) NOT NULL,
                      code VARCHAR(100) UNIQUE NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS sys.product_attribute_values (
                      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                      product_id UUID NOT NULL REFERENCES sys.products(id) ON DELETE CASCADE,
                      attribute_id UUID NOT NULL REFERENCES sys.product_attributes(id) ON DELETE CASCADE,
                      value_text VARCHAR(150) NOT NULL,
                      UNIQUE(product_id, attribute_id, value_text)
                    );

                    CREATE TABLE IF NOT EXISTS sys.product_variants (
                      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                      product_id UUID NOT NULL REFERENCES sys.products(id) ON DELETE CASCADE,
                      sku VARCHAR(100) UNIQUE,
                      price DECIMAL(12,2),
                      stock_quantity DECIMAL(10,3),
                      is_active BOOLEAN DEFAULT TRUE,
                      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS sys.product_variant_attribute_values (
                      variant_id UUID NOT NULL REFERENCES sys.product_variants(id) ON DELETE CASCADE,
                      attribute_id UUID NOT NULL REFERENCES sys.product_attributes(id) ON DELETE CASCADE,
                      value_text VARCHAR(150) NOT NULL,
                      PRIMARY KEY (variant_id, attribute_id, value_text)
                    );

                  DO $$
                    BEGIN
                      IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'chk_users_no_self_referral'
                          AND conrelid = 'sys.users'::regclass
                      ) THEN
                        ALTER TABLE sys.users
                          ADD CONSTRAINT chk_users_no_self_referral
                          CHECK (referred_by IS NULL OR referred_by <> id);
                      END IF;
                END$$;

                    CREATE OR REPLACE VIEW sys.vw_products_search AS
                    SELECT
                      p.id,
                      p.slug,
                      COALESCE(pt_tr.name,  pt_any.name)  AS name,
                      COALESCE(pt_tr.description, pt_any.description) AS description,
                      p.price,
                      p.status,
                      p.brand_id,
                      p.category_id,
                      p.vendor_id,
                      p.created_at
                    FROM sys.products p
                    LEFT JOIN LATERAL (
                      SELECT name, description
                      FROM sys.product_translations
                      WHERE product_id = p.id AND language_code = 'tr'
                      LIMIT 1
                    ) pt_tr ON TRUE
                    LEFT JOIN LATERAL (
                      SELECT name, description
                      FROM sys.product_translations
                      WHERE product_id = p.id
                      ORDER BY language_code
                      LIMIT 1
                    ) pt_any ON TRUE
                    WHERE p.is_deleted = FALSE;

                    CREATE INDEX IF NOT EXISTS idx_brands_status ON sys.brands(status);
                    CREATE INDEX IF NOT EXISTS idx_products_brand ON sys.products(brand_id);
                    CREATE INDEX IF NOT EXISTS idx_products_category ON sys.products(category_id);
                    CREATE INDEX IF NOT EXISTS idx_products_vendor   ON sys.products(vendor_id);
                    CREATE INDEX IF NOT EXISTS idx_products_vendor_status ON sys.products(vendor_id, status);
                    CREATE INDEX IF NOT EXISTS idx_products_status   ON sys.products(status) WHERE is_deleted = FALSE;
                    CREATE INDEX IF NOT EXISTS idx_products_active   ON sys.products(id) WHERE is_deleted = FALSE AND status = 'ACTIVE';
                    CREATE INDEX IF NOT EXISTS idx_product_trans_pid_lang ON sys.product_translations(product_id, language_code);

                    CREATE TABLE IF NOT EXISTS sys.discounts (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    code VARCHAR(50) UNIQUE NOT NULL, -- Kupon kodu (örneğin: WELCOME10)
                    discount_type discount_type_enum,
                    discount_value DECIMAL(10,2) NOT NULL, -- Yüzde ise 10, sabit ise 50₺ gibi

                    target_type discount_target_enum,
                    target_value UUID, -- ürün/kategori ID'si (PRODUCT veya CATEGORY ile eşleşir)

                    min_cart_total DECIMAL(10,2), -- Opsiyonel: minimum tutar şartı
                    usage_limit INT,              -- Kupon toplamda kaç kez kullanılabilir
                    used_count INT DEFAULT 0,     -- Şu ana kadar kaç kez kullanıldı
                    is_single_use_per_user BOOLEAN DEFAULT FALSE,

                    valid_from TIMESTAMP,
                    valid_until TIMESTAMP,
                    is_active BOOLEAN DEFAULT TRUE,

                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );

                    -- Buyers
                    CREATE TABLE IF NOT EXISTS sys.buyers (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    buyer_type buyer_type_enum,
                    name VARCHAR(100),
                    email VARCHAR(100) UNIQUE,
                    phone VARCHAR(20),
                    company_name VARCHAR(200),
                    tax_number VARCHAR(50),
                    tax_office VARCHAR(100),
                    description TEXT,
                    linked_user_id UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );

                    CREATE TABLE IF NOT EXISTS sys.orders (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

                    -- Satıcı ve alıcı
                    vendor_id UUID NOT NULL REFERENCES sys.vendors(id) ON DELETE RESTRICT,
                    buyer_id UUID REFERENCES sys.buyers(id) ON DELETE RESTRICT,
                    buyer_description VARCHAR(255), -- fatura ad/soyad gibi ek açıklama

                    -- Finansal Bilgiler
                    total_amount DECIMAL(12,2) NOT NULL,
                    currency VARCHAR(10) DEFAULT 'TRY',

                    -- Sipariş Durumu
                    order_status order_status_enum DEFAULT 'PENDING',
                    discount_code VARCHAR(50),
                    discount_amount DECIMAL(12,2) DEFAULT 0,
                    -- Ödeme Durumu
                    payment_status payment_status_enum DEFAULT 'AWAITING',

                    -- Tarihler
                    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,

                    -- Silme işlemleri
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT,

                    -- Kullanıcılar
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL
                    );


                    CREATE TABLE IF NOT EXISTS sys.discount_usages (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    discount_id UUID REFERENCES sys.discounts(id) ON DELETE CASCADE,
                    user_id UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    order_id UUID REFERENCES sys.orders(id) ON DELETE SET NULL,
                    used_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );


                    -- Suppliers
                    CREATE TABLE IF NOT EXISTS sys.suppliers (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    name VARCHAR(200) UNIQUE NOT NULL,
                    contact_person VARCHAR(100),
                    email VARCHAR(100),
                    phone VARCHAR(20),
                    address TEXT,
                    tax_number VARCHAR(50),
                    tax_office VARCHAR(100),
                    supplier_type supplier_type_enum,
                    status status_enum,
                    payment_terms VARCHAR(100),
                    credit_limit DECIMAL(12,2),
                    notes TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );

                    -- 📦 Kargo Süreci: Alıcı Adresi
                    CREATE TABLE IF NOT EXISTS sys.buyer_addresses (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    buyer_id UUID NOT NULL REFERENCES sys.buyers(id) ON DELETE CASCADE,
                    address_title VARCHAR(100),
                    full_address TEXT,
                    city VARCHAR(100),
                    postal_code VARCHAR(20),
                    country VARCHAR(100),
                    is_default BOOLEAN DEFAULT FALSE,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );

                    CREATE TABLE IF NOT EXISTS sys.orders_details (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

                    -- Satış ve ürün ilişkisi
                    order_id UUID NOT NULL REFERENCES sys.orders(id) ON DELETE CASCADE,
                    product_id UUID NOT NULL REFERENCES sys.products(id) ON DELETE RESTRICT,
                    invoice_address_id UUID REFERENCES sys.buyer_addresses(id) ON DELETE SET NULL,

                    -- Miktar ve fiyat
                    quantity DECIMAL(10,3) NOT NULL,
                    unit_price DECIMAL(12,2) NOT NULL,
                    discount DECIMAL(12,2) DEFAULT 0,

                    -- Hesaplanmış toplam
                    total_price DECIMAL(12,2) GENERATED ALWAYS AS (quantity * unit_price - discount) STORED,

                    -- Ek bilgiler
                    note TEXT,

                    -- Tarihler
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,

                    -- Silme (soft delete yapılabilir)
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,

                    -- Kullanıcılar
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,

                    -- Unique constraint: aynı ürün bir siparişte bir kez görünür
                    UNIQUE (order_id, product_id)
                    );

                    CREATE TABLE IF NOT EXISTS sys.returns (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    order_id UUID NOT NULL REFERENCES sys.orders(id) ON DELETE CASCADE,
                    order_detail_id UUID REFERENCES sys.orders_details(id) ON DELETE SET NULL,
                    buyer_id UUID NOT NULL REFERENCES sys.buyers(id) ON DELETE RESTRICT,
                    reason TEXT NOT NULL,
                    status return_status_enum DEFAULT 'PENDING',
                    processed_at TIMESTAMP,
                    processed_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );

            

                   

                    -- 📦 Kargo Süreci: Gönderi Bilgileri
                    CREATE TABLE IF NOT EXISTS sys.shipments (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    order_id UUID NOT NULL REFERENCES sys.orders(id) ON DELETE CASCADE,
                    address_id UUID REFERENCES sys.buyer_addresses(id) ON DELETE SET NULL,
                    tracking_number VARCHAR(100),
                    shipping_provider VARCHAR(100),
                    shipment_status shipment_status_enum DEFAULT 'PREPARING',
                    shipped_at TIMESTAMP,
                    delivered_at TIMESTAMP,
                    notes TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );

                    -- 💳 Ödeme Tipleri
                    CREATE TABLE IF NOT EXISTS sys.payment_methods (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    name VARCHAR(100) UNIQUE NOT NULL,
                    description TEXT,
                    is_active BOOLEAN DEFAULT TRUE,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );

                    -- 💳 Ödeme İşlemleri (Web3 ile entegre olacak)
                    CREATE TABLE IF NOT EXISTS sys.payments (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    order_id UUID NOT NULL REFERENCES sys.orders(id) ON DELETE CASCADE,
                    payment_method_id UUID REFERENCES sys.payment_methods(id) ON DELETE SET NULL,
                    payment_status payment_status_enum DEFAULT 'AWAITING',
                    transaction_hash VARCHAR(200),
                    paid_at TIMESTAMP,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );

                    CREATE TABLE IF NOT EXISTS sys.stock_movements (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    product_id UUID NOT NULL REFERENCES sys.products(id),
                    supplier_id UUID REFERENCES sys.suppliers(id) ON DELETE SET NULL,
                    movement_type movement_type_enum NOT NULL,
                    quantity DECIMAL(10,3) NOT NULL CHECK (quantity > 0),
                    movement_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    description TEXT,
                    related_order_id UUID REFERENCES sys.orders(id) ON DELETE SET NULL,
                    status status_enum DEFAULT 'ACTIVE',
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_at TIMESTAMP,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );


                    -- 🧾 Fatura Bilgileri (Opsiyonel)
                    CREATE TABLE IF NOT EXISTS sys.invoices (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    order_id UUID NOT NULL REFERENCES sys.orders(id) ON DELETE CASCADE,
                    invoice_number VARCHAR(100) UNIQUE,
                    issued_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    total_amount DECIMAL(12,2),
                    currency VARCHAR(10) DEFAULT 'TRY',
                    buyer_address_id UUID REFERENCES sys.buyer_addresses(id) ON DELETE RESTRICT,

                    pdf_url VARCHAR(500),
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,
                    created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                    delete_reason TEXT
                    );


                    -- 🔄 Sipariş Durum Logları
                    CREATE TABLE IF NOT EXISTS sys.order_status_log (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    order_id UUID NOT NULL REFERENCES sys.orders(id) ON DELETE CASCADE,
                    old_status order_status_enum,
                    new_status order_status_enum,
                    notes TEXT,
                    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    changed_by UUID REFERENCES sys.users(id) ON DELETE SET NULL
                    );
                    -- Sepet tablosu
                    CREATE TABLE IF NOT EXISTS sys.carts (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    user_id UUID REFERENCES sys.users(id) ON DELETE CASCADE,
                    cart_status cart_status_enum DEFAULT 'ACTIVE',
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP,
                    expires_at TIMESTAMP
                    );

                    -- Her kullanıcı için sadece 1 aktif sepet
                    CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_active_cart_per_user
                    ON sys.carts (user_id)
                    WHERE cart_status = 'ACTIVE';
                    CREATE TABLE IF NOT EXISTS sys.cart_items (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    cart_id UUID REFERENCES sys.carts(id) ON DELETE CASCADE,
                    product_id UUID REFERENCES sys.products(id) ON DELETE CASCADE,
                    quantity DECIMAL(10,3) NOT NULL,
                    unit_price DECIMAL(18,2) NOT NULL,
                    currency VARCHAR(10) NOT NULL,
                    status status_enum DEFAULT 'ACTIVE',
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    created_by UUID,
                    updated_at TIMESTAMP,
                    updated_by UUID,
                    is_deleted BOOLEAN DEFAULT FALSE,
                    deleted_at TIMESTAMP,
                    deleted_by UUID,
                    delete_reason TEXT,
                    UNIQUE(cart_id, product_id)
                    );

                    
                    CREATE TABLE IF NOT EXISTS sys.referral_rewards (
                        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

                        -- Kullanıcı ilişkileri
                        referred_user_id UUID NOT NULL REFERENCES sys.users(id) ON DELETE CASCADE,    -- Sisteme referansla gelen kullanıcı
                        referrer_user_id UUID NOT NULL REFERENCES sys.users(id) ON DELETE CASCADE,    -- Referanslayan kullanıcı

                        -- Ödül bilgileri
                        reward_type VARCHAR(50) NOT NULL,                  -- Örnek: 'DISCOUNT', 'POINT', 'BONUS_CREDIT'
                        reward_amount DECIMAL(10,2),                       -- Ödül miktarı
                        reward_currency VARCHAR(10),                       -- 'TRY', 'POINT' gibi
                        reward_description TEXT,                           -- Açıklama (yöneticiye/raporlara)

                        -- Durum bilgileri
                        reward_status referral_reward_status_enum DEFAULT 'PENDING',  -- ENUM: 'PENDING', 'GRANTED', 'EXPIRED', 'REJECTED'
                        triggered_event VARCHAR(100),                      -- Tetikleyici olay: 'REGISTRATION', 'FIRST_PURCHASE'
                        is_reward_granted BOOLEAN DEFAULT FALSE,           -- Ödül gerçekten verildi mi?

                        -- Zaman & kontrol
                        reward_expiration_date TIMESTAMP,                  -- Ödülün geçerlilik süresi varsa
                        granted_at TIMESTAMP,                              -- Ödül ne zaman verildi
                        granted_by UUID REFERENCES sys.users(id) ON DELETE SET NULL, -- Manuel ödül veren kullanıcı (admin)
                        reward_source VARCHAR(50),                         -- 'SYSTEM', 'ADMIN_PANEL', 'CRON' vb.

                        -- Denetim
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP,
                        created_by UUID REFERENCES sys.users(id) ON DELETE SET NULL,
                        updated_by UUID REFERENCES sys.users(id) ON DELETE SET NULL
                    );

                    -- Product Category Path View
                    CREATE OR REPLACE VIEW sys.vw_product_category_paths AS
                    WITH RECURSIVE category_path (id, name, parent_category_id, path) AS (
                    SELECT id, name, parent_category_id, name::TEXT
                    FROM sys.product_categories
                    WHERE parent_category_id IS NULL

                    UNION ALL

                    SELECT c.id, c.name, c.parent_category_id, cp.path || ' > ' || c.name
                    FROM sys.product_categories c
                    INNER JOIN category_path cp ON c.parent_category_id = cp.id
                    )
                    SELECT * FROM category_path;

                    CREATE OR REPLACE VIEW sys.vw_vendor_orders_summary AS
                SELECT 
                p.vendor_id AS product_vendor_id,
                s.vendor_id AS order_vendor_id,
                COUNT(sd.id) as total_items_sold,
                SUM(sd.total_price) as total_revenue
                    FROM sys.orders s
                    JOIN sys.orders_details sd ON s.id = sd.order_id
                    JOIN sys.products p ON sd.product_id = p.id
                    GROUP BY p.vendor_id, s.vendor_id;

                    CREATE OR REPLACE VIEW sys.vw_user_referral_chain AS
                    WITH RECURSIVE referral_chain AS (
                      SELECT 
                        id,
                        referred_by,
                        0 as level,
                        id::TEXT as chain
                      FROM sys.users
                      WHERE referred_by IS NULL

                      UNION ALL

                      SELECT 
                        u.id,
                        u.referred_by,
                        rc.level + 1,
                        rc.chain || ' → ' || u.id
                      FROM sys.users u
                      JOIN referral_chain rc ON u.referred_by = rc.id
                    )
                    SELECT * FROM referral_chain;

                CREATE INDEX IF NOT EXISTS idx_audit_user_date ON sys.audit_logs(user_id, created_at);

";

        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }

    public static void DeleteDatabase(IConfiguration configuration)
    {
        var provider = configuration["DatabaseConnection:Provider"];
        if (provider != "PostgreSql") return;

        var connectionString = configuration["DatabaseConnection:ConnectionString"];
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var sql = @"
            DO $$
BEGIN
  -- 1) sys şemasını komple düşür (içindeki tüm tablolar, view'lar, FK'lar, index'ler vs. ile birlikte)
  IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'sys') THEN
    EXECUTE 'DROP SCHEMA sys CASCADE';
  END IF;

  -- 2) ENUM tiplerini düşür (artık hiçbir tablo referans vermiyor olmalı; yine de CASCADE güvenlik için)
  PERFORM 1
  FROM pg_type t
  WHERE t.typname IN (
    'status_enum',
    'kyc_status_enum',
    'document_type_enum',
    'buyer_type_enum',
    'product_type_enum',
    'order_status_enum',
    'payment_status_enum',
    'supplier_type_enum',
    'shipment_status_enum',
    'discount_type_enum',
    'discount_target_enum',
    'cart_status_enum',
    'return_status_enum',
    'permission_action_enum',
    'movement_type_enum',
    'referral_reward_status_enum'
  );

  -- ENUM'ları tek tek, varsa düşür
  EXECUTE 'DROP TYPE IF EXISTS status_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS kyc_status_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS document_type_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS buyer_type_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS product_type_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS order_status_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS payment_status_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS supplier_type_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS shipment_status_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS discount_type_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS discount_target_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS cart_status_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS return_status_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS permission_action_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS movement_type_enum CASCADE';
  EXECUTE 'DROP TYPE IF EXISTS referral_reward_status_enum CASCADE';
END;
$$;

            ";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }
    public static void SeedData(IConfiguration configuration)
    {
        var provider = configuration["DatabaseConnection:Provider"];
        if (provider != "PostgreSql") return;

        var connectionString = configuration["DatabaseConnection:ConnectionString"];
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var commands = new[]
        {
            @"INSERT INTO sys.users (id, username, first_name, last_name, email, phone, password_hash, status, created_at)
              VALUES ('00000000-0000-0000-0000-000000000001','system','System','User','system@localhost','+900000000000','seedhash','ACTIVE', now())
              ON CONFLICT (id) DO NOTHING;",

            @"INSERT INTO sys.resources (code, name, description, created_by, created_at)
              VALUES
                ('USERS','USERS','Users operations','00000000-0000-0000-0000-000000000001', now()),
                ('ROLES','ROLES','Roles operations','00000000-0000-0000-0000-000000000001', now()),
                ('PERMISSIONS','PERMISSIONS','Permissions operations','00000000-0000-0000-0000-000000000001', now())
              ON CONFLICT (code) DO NOTHING;",

            @"INSERT INTO sys.permissions (code, group_name, name, description, resource_id, action, created_by, created_at)
              VALUES
                ('USERS.VIEW','USERS','USERS.VIEW','View users',(SELECT id FROM sys.resources WHERE code = 'USERS'),'VIEW','00000000-0000-0000-0000-000000000001', now()),
                ('ROLES.VIEW','ROLES','ROLES.VIEW','View roles',(SELECT id FROM sys.resources WHERE code = 'ROLES'),'VIEW','00000000-0000-0000-0000-000000000001', now()),
                ('PERMISSIONS.VIEW','PERMISSIONS','PERMISSIONS.VIEW','View permissions',(SELECT id FROM sys.resources WHERE code = 'PERMISSIONS'),'VIEW','00000000-0000-0000-0000-000000000001', now())
              ON CONFLICT (code) DO NOTHING;",

            @"INSERT INTO sys.roles (name, description, status, created_by, created_at)
              VALUES ('SYSTEM_ADMIN','System administrator','ACTIVE','00000000-0000-0000-0000-000000000001', now())
              ON CONFLICT (name) DO NOTHING;",

            @"INSERT INTO sys.role_permissions (role_id, permission_id, created_by, created_at)
              SELECT r.id, p.id, '00000000-0000-0000-0000-000000000001', now()
              FROM sys.roles r, sys.permissions p
              WHERE r.name = 'SYSTEM_ADMIN' AND p.code IN ('USERS.VIEW','ROLES.VIEW','PERMISSIONS.VIEW')
              ON CONFLICT (role_id, permission_id) DO NOTHING;",

            @"INSERT INTO sys.user_roles (user_id, role_id, created_by, created_at)
              SELECT '00000000-0000-0000-0000-000000000001', id, '00000000-0000-0000-0000-000000000001', now()
              FROM sys.roles
              WHERE name = 'SYSTEM_ADMIN'
              ON CONFLICT (user_id, role_id) DO NOTHING;"
        };

        foreach (var sql in commands)
        {
            using var cmdSeed = new NpgsqlCommand(sql, connection);
            cmdSeed.ExecuteNonQuery();
        }
    }
}
