namespace Sky.Template.Backend.Infrastructure.Queries;

public static class SelfUserQueries
{
    public const string GetSelfProfile = @"\
WITH base_user AS (\
  SELECT u.id AS \"Id\",\
         u.first_name AS \"FirstName\",\
         u.last_name AS \"LastName\",\
         u.email AS \"Email\",\
         u.phone AS \"Phone\",\
         u.image_path AS \"UserImagePath\",\
         u.status AS \"Status\",\
         u.last_login_at AS \"LastLoginAt\",\
         u.password_changed_at AS \"PasswordChangedAt\"\
  FROM sys.users u\
  WHERE u.id = @userId\
), role_cte AS (\
  SELECT r.id AS \"Id\", r.name AS \"Name\", r.description AS \"Description\"\
  FROM sys.roles r\
  JOIN sys.user_roles ur ON ur.role_id = r.id\
  WHERE ur.user_id = @userId\
  LIMIT 1\
), perm_cte AS (\
  SELECT DISTINCT p.code\
  FROM sys.permissions p\
  JOIN sys.role_permissions rp ON rp.permission_id = p.id\
  JOIN sys.user_roles ur ON ur.role_id = rp.role_id\
  WHERE ur.user_id = @userId\
), vendor_cte AS (\
  SELECT v.id AS \"Id\", v.name AS \"Name\", v.slug AS \"Slug\", v.logo_path AS \"LogoPath\", v.kyc_status AS \"KycStatus\", v.status AS \"Status\"\
  FROM sys.vendors v\
  JOIN sys.vendor_users vu ON vu.vendor_id = v.id\
  WHERE vu.user_id = @userId\
  LIMIT 1\
), kyc_cte AS (\
  SELECT k.kyc_status AS \"KycStatus\", k.expires_at AS \"ExpiresAt\", k.last_reviewed_at AS \"LastReviewedAt\"\
  FROM sys.kyc_verifications k\
  WHERE k.user_id = @userId\
  ORDER BY k.last_reviewed_at DESC\
  LIMIT 1\
), addr_cte AS (\
  SELECT a.id AS \"Id\", a.title AS \"Title\", a.country AS \"Country\", a.city AS \"City\", a.district AS \"District\", a.postal_code AS \"PostalCode\", a.address_line AS \"AddressLine\", a.is_default AS \"IsDefault\", a.status AS \"Status\"\
  FROM sys.user_addresses a\
  WHERE a.user_id = @userId AND a.status <> 'DELETED'\
), pref_cte AS (\
  SELECT p.language AS \"Language\", p.currency AS \"Currency\", p.theme AS \"Theme\", p.time_zone AS \"TimeZone\"\
  FROM sys.user_preferences p\
  WHERE p.user_id = @userId\
  LIMIT 1\
), notif_cte AS (\
  SELECT n.email_notifications AS \"EmailNotifications\", n.sms_notifications AS \"SmsNotifications\", n.push_notifications AS \"PushNotifications\", n.newsletter_opt_in AS \"NewsletterOptIn\"\
  FROM sys.user_notification_settings n\
  WHERE n.user_id = @userId\
  LIMIT 1\
), sess_cte AS (\
  SELECT s.id AS \"Id\", s.device AS \"Device\", s.user_agent AS \"UserAgent\", s.ip_address AS \"IpAddress\", s.created_at AS \"CreatedAt\", s.last_seen_at AS \"LastSeenAt\", s.is_current AS \"IsCurrent\"\
  FROM sys.user_sessions s\
  WHERE s.user_id = @userId\
)
SELECT\
  bu.*,\
  (SELECT row_to_json(role_cte) FROM role_cte) AS \"Role\",\
  (SELECT COALESCE(array_agg(perm_cte.code), '{}')) AS \"PermissionCodes\",\
  (SELECT row_to_json(vendor_cte) FROM vendor_cte) AS \"Vendor\",\
  (SELECT row_to_json(kyc_cte) FROM kyc_cte) AS \"Kyc\",\
  (SELECT COALESCE(json_agg(addr_cte), '[]')) AS \"Addresses\",\
  (SELECT row_to_json(pref_cte) FROM pref_cte) AS \"Preferences\",\
  (SELECT row_to_json(notif_cte) FROM notif_cte) AS \"Notifications\",\
  (SELECT COALESCE(json_agg(sess_cte), '[]')) AS \"Sessions\"\
FROM base_user bu;";

    public const string GetSelfPermissionCodes = @"\
        SELECT DISTINCT p.code\
        FROM sys.permissions p\
        JOIN sys.role_permissions rp ON rp.permission_id = p.id\
        JOIN sys.user_roles ur ON ur.role_id = rp.role_id\
        WHERE ur.user_id = @userId";

    public const string GetSelfAddresses = @"\
        SELECT a.id AS \"Id\", a.title AS \"Title\", a.country AS \"Country\", a.city AS \"City\", a.district AS \"District\", a.postal_code AS \"PostalCode\", a.address_line AS \"AddressLine\", a.is_default AS \"IsDefault\", a.status AS \"Status\"\
        FROM sys.user_addresses a\
        WHERE a.user_id = @userId AND a.status <> 'DELETED'";

    public const string GetSelfSessions = @"\
        SELECT s.id AS \"Id\", s.device AS \"Device\", s.user_agent AS \"UserAgent\", s.ip_address AS \"IpAddress\", s.created_at AS \"CreatedAt\", s.last_seen_at AS \"LastSeenAt\", s.is_current AS \"IsCurrent\"\
        FROM sys.user_sessions s\
        WHERE s.user_id = @userId";

    public const string RevokeSelfSession = @"DELETE FROM sys.user_sessions WHERE user_id = @userId AND id = @sessionId";

    public const string UpsertSelfNotifications = @"\
        INSERT INTO sys.user_notification_settings (user_id, email_notifications, sms_notifications, push_notifications, newsletter_opt_in)
        VALUES (@userId, @EmailNotifications, @SmsNotifications, @PushNotifications, @NewsletterOptIn)
        ON CONFLICT (user_id) DO UPDATE SET
            email_notifications = EXCLUDED.email_notifications,
            sms_notifications = EXCLUDED.sms_notifications,
            push_notifications = EXCLUDED.push_notifications,
            newsletter_opt_in = EXCLUDED.newsletter_opt_in,
            updated_at = NOW();";

    public const string UpsertSelfPreferences = @"\
        INSERT INTO sys.user_preferences (user_id, language, currency, theme, time_zone)
        VALUES (@userId, @Language, @Currency, @Theme, @TimeZone)
        ON CONFLICT (user_id) DO UPDATE SET
            language = EXCLUDED.language,
            currency = EXCLUDED.currency,
            theme = EXCLUDED.theme,
            time_zone = EXCLUDED.time_zone,
            updated_at = NOW();";

    public const string UpdateSelfProfile = @"\
        UPDATE sys.users
        SET first_name = @FirstName,
            last_name = @LastName,
            phone = @Phone,
            updated_at = NOW()
        WHERE id = @userId
        RETURNING *";
}
