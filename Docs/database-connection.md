
# 📄 DatabaseConnection Yapılandırması (appsettings.json)

Bu projede veri tabanı sağlayıcısı dinamik olarak seçilebilecek şekilde tasarlanmıştır. Uygulama, `appsettings.json` dosyasındaki `DatabaseConnection` konfigürasyonuna göre uygun `DbProviderFactory` kullanarak veri tabanına bağlanır.

---

## 🔧 appsettings.json Örneği

```json
{
  "DatabaseConnection": {
    "Provider": "PostgreSql",
    "ConnectionString": "Host=localhost;Port=5432;Username=postgres;Password=your_password;Database=mydb"
  }
}
```

---

## 🎯 Desteklenen `Provider` Değerleri

| Sağlayıcı Adı | Açıklama            | NuGet Paketi                  |
|---------------|---------------------|-------------------------------|
| SqlServer     | Microsoft SQL Server| `Microsoft.Data.SqlClient`   |
| PostgreSql    | PostgreSQL          | `Npgsql`                      |
| MySql         | MySQL               | `MySql.Data`                  |

---

## 🔐 Örnek Connection String’ler

### ✅ PostgreSQL

```json
"ConnectionString": "Host=localhost;Port=5432;Username=postgres;Password=your_password;Database=mydb"
```

### ✅ SQL Server

```json
"ConnectionString": "Server=localhost;Database=mydb;User Id=sa;Password=your_password;TrustServerCertificate=true;"
```

### ✅ MySQL

```json
"ConnectionString": "Server=localhost;Database=mydb;User=myuser;Password=your_password;"
```

---

## 🧪 Bağlantıyı Doğrulama

Uygulamanın doğru sağlayıcıyı seçip veri tabanına bağlanabildiğini doğrulamak için:

```bash
dotnet run
```

> Eğer bağlantı hatası alıyorsanız:
>
> - `ConnectionString` bilgilerini kontrol edin.
> - Veri tabanı servisinin çalıştığından emin olun.
> - `Provider` alanının desteklenen bir değer içerdiğini kontrol edin.

---

## 📌 Notlar

- Tüm bağlantılar `DbProviderFactory` ile soyutlanmıştır.
- Geliştirme ortamı için `appsettings.Development.json` içinde override edebilirsiniz.
- `ReplaceSchemaPlaceholder` fonksiyonu ile `$db.` yer tutucu dinamik olarak şema ismiyle değiştirilir.
