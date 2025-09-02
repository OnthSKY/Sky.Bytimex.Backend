# Sky Template Backend - PostgreSQL SYS Schema Documentation

Bu belge, Sky Template Backend projesi için PostgreSQL üzerinde tanımlanan SYS şema yapısının geliştirici dokümanıdır.

---

## 🔹 Genel Bilgiler

* **Veritabanı Motoru:** PostgreSQL
* **Şema:** `sys`
* **UUID Kullanılan Tablolar:**

  * `users`
  * `audit_logs`
  * `error_logs`
  * `refresh_tokens`
* **SERIAL (INT) Kullanılan Tablolar:**

  * `roles`
  * `permissions`
  * `user_roles`
  * `role_permissions`

---

## 📊 Tablolar ve Alanlar

### sys.users

| Alan           | Tip          | Not                     |
|----------------|--------------| ----------------------- |
| id             | UUID         | PK, Default gen\_random\_uuid() |
| username       | VARCHAR(50)  | UNIQUE                  |
| email          | VARCHAR(100) | UNIQUE                  |
| password\_hash | VARCHAR(200) |                         |
| status         | VARCJAR(200) |                     |
| created\_at    | TIMESTAMP    | Default CURRENT\_TIMESTAMP |

### sys.roles

| Alan        | Tip         | Not    |
| ----------- | ----------- | ------ |
| id          | SERIAL      | PK     |
| name        | VARCHAR(50) | UNIQUE |
| description | TEXT        |        |

### sys.permissions

| Alan        | Tip         | Not    |
| ----------- | ----------- | ------ |
| id          | SERIAL      | PK     |
| name        | VARCHAR(50) | UNIQUE |
| description | TEXT        |        |

### sys.user\_roles

| Alan        | Tip                  | Not         |
| ----------- | -------------------- | ----------- |
| user\_id    | UUID                 | FK users.id |
| role\_id    | INT                  | FK roles.id |
| PRIMARY KEY | (user\_id, role\_id) |             |

### sys.role\_permissions

| Alan           | Tip                        | Not               |
| -------------- | -------------------------- | ----------------- |
| role\_id       | INT                        | FK roles.id       |
| permission\_id | INT                        | FK permissions.id |
| PRIMARY KEY    | (role\_id, permission\_id) |                   |

### sys.audit\_logs

| Alan        | Tip          | Not                        |
| ----------- | ------------ | -------------------------- |
| id          | UUID         | PK, gen\_random\_uuid()    |
| user\_id    | UUID         | FK users.id                |
| action      | VARCHAR(100) |                            |
| table\_name | VARCHAR(100) |                            |
| record\_id  | VARCHAR(100) |                            |
| old\_values | JSONB        |                            |
| new\_values | JSONB        |                            |
| created\_at | TIMESTAMP    | Default CURRENT\_TIMESTAMP |

### sys.error\_logs

| Alan            | Tip          | Not                        |
| --------------- | ------------ | -------------------------- |
| id              | UUID         | PK, gen\_random\_uuid()    |
| message         | TEXT         |                            |
| stack\_trace    | TEXT         |                            |
| source          | VARCHAR(100) |                            |
| trace\_id       | VARCHAR(100) |                            |
| user\_name      | VARCHAR(100) |                            |
| client\_ip      | VARCHAR(50)  |                            |
| exception\_type | VARCHAR(100) |                            |
| created\_at     | TIMESTAMP    | Default CURRENT\_TIMESTAMP |

### sys.refresh\_tokens

| Alan             | Tip          | Not                           |
| ---------------- | ------------ | ----------------------------- |
| id               | UUID         | PK, gen\_random\_uuid()       |
| token            | VARCHAR(500) | NOT NULL                      |
| user\_id         | UUID         | FK users.id ON DELETE CASCADE |
| expiration\_date | TIMESTAMP    | NOT NULL                      |
| schema\_name     | VARCHAR(100) |                               |
| created\_at      | TIMESTAMP    | Default CURRENT\_TIMESTAMP    |

---

## 🗕️ Bağlı PostgreSQL Extension

```sql
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
```

---

## 📌 Kod Uygulaması

.NET projesinde bu tabloları oluşturan initializer mevcuttur. Kullanım örneği:

```csharp
DatabaseInitializer.InitializeDatabase(builder.Configuration);
```

---

## 🔹 Notlar

* UUID ile şifrelenmiş id yapıları, güvenlik ve dağıtık sistemler için uygundur.
* Rollerin ve izinlerin sabit olması nedeniyle INT tercih edilmiştir.
* Audit ve error logları UUID ile benzersiz kayıt sağlar.
* Refresh token yönetimi için `sys.refresh_tokens` tablosu eklenmiştir. Kullanıcı silindiğinde tokenlar da otomatik silinir.
* `schema_name` alanı çoklu şema veya kullanım ayrımı için esnek bırakılmıştır.

---

> Son güncelleme: 2025-07-15
