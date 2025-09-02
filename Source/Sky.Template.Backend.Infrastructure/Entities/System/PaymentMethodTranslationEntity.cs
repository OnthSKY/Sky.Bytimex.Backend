using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.System;

[TableName("payment_method_translations")]
public class PaymentMethodTranslationEntity
{
    [DbManager.mColumn("id")]
    public Guid Id { get; set; }

    [DbManager.mColumn("payment_method_id")]
    public Guid PaymentMethodId { get; set; }

    [DbManager.mColumn("language_code")]
    public string LanguageCode { get; set; } = string.Empty;

    [DbManager.mColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.mColumn("description")]
    public string? Description { get; set; }
}
