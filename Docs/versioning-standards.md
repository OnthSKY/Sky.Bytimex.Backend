
# 📌 API Versioning Standards / API Versiyonlama Standartları

This document describes how API versioning is implemented and maintained.  
Bu doküman, API'de versiyonlamanın nasıl uygulanacağını ve sürdürüleceğini açıklar.

---

## 🎯 Strategy / Strateji

- Versioning is done via **URL route**: `/api/v1/`, `/api/v2/`  
- Versiyonlama **URL üzerinden (route tabanlı)** yapılır: `/api/v1/`, `/api/v2/`

- Each controller must define `ApiVersion`.  
- Her controller `ApiVersion` tanımlamalıdır.

- New versions are only introduced for **breaking changes**.  
- Yeni versiyonlar sadece **geriye uyumsuz (breaking)** değişikliklerde eklenir.

- Backward-compatible changes stay within the same version.  
- Geriye uyumlu değişiklikler (yeni alan eklemek gibi) aynı versiyonda kalır.

---

## 📁 Folder Structure / Klasör Yapısı

```
Docs/
├── versioning-standards.md     # Versioning rules / Versiyon kuralları
├── api-v1.md                   # v1 endpoints
├── api-v2.md                   # v2 endpoints
```

---

## 🧱 Controller Rules / Controller Kuralları

```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ExpensesController : ControllerBase
{
    [MapToApiVersion("1.0")]
    public IActionResult GetPaged() { }

    [MapToApiVersion("2.0")]
    [HttpGet("all")]
    public IActionResult GetAll() { }
}
```

- `MapToApiVersion` must be used for each action method.  
- Her method için `MapToApiVersion` kullanılmalıdır.

---

## 🧪 Program.cs API Versioning Config / Versiyonlama Ayarı

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader()
    );
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

---

## 🧾 Swagger & Documentation / Swagger & Dokümantasyon

- Each version should be visible in Swagger.  
- Her versiyon Swagger'da ayrı ayrı görünmelidir.

```csharp
app.UseSwaggerUI(options =>
{
    foreach (var description in apiVersionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                                $"Sky Template API {description.GroupName.ToUpperInvariant()}");
    }
});
```

- Swagger definitions are version-specific:
```csharp
options.SwaggerDoc(description.GroupName, new OpenApiInfo
{
    Title = "Sky Template API",
    Version = description.ApiVersion.ToString(),
    Description = "Masraf yönetimi API'si"
});
```

- Use `Docs/api-vX.md` files for detailed contract examples.  
- Detaylı endpoint yapıları `Docs/api-vX.md` dosyalarına yazılmalıdır.

---

## 🧮 When to Create a New Version? / Yeni Versiyon Ne Zaman Açılır?

| Change / Değişiklik                        | New Version? / Yeni Versiyon? |
|--------------------------------------------|-------------------------------|
| New endpoint added / Yeni endpoint eklendi | ❌ No / Hayır                 |
| New field added to response / Yeni alan    | ❌ No / Hayır                 |
| Existing field changed / Alan değişti      | ✅ Yes / Evet                 |
| Field removed / Alan silindi               | ✅ Yes / Evet                 |
| Behavior change / Davranış değişti         | ✅ Yes / Evet                 |

---

## ✅ Notes / Notlar

- Versions should not be deleted. Use deprecation warnings.  
- Versiyonlar silinmez. "Deprecated" olarak işaretlenir.

- Use clear commit messages:  
  Açık commit mesajları kullanın:
  ```bash
  feat(api-v2): added GetAllExpenses endpoint
  fix(api-v1): fixed pagination bug
  ```

---

Last updated: April 2025  
Prepared by: SKY