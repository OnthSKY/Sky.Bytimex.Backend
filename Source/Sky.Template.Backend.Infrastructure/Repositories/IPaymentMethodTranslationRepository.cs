using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IPaymentMethodTranslationRepository
{
    Task<PaymentMethodTranslationEntity?> GetAsync(Guid paymentMethodId, string languageCode);
    Task<PaymentMethodTranslationEntity> UpsertAsync(PaymentMethodTranslationEntity entity, DbConnection? connection = null, DbTransaction? transaction = null);
}

public class PaymentMethodTranslationRepository : IPaymentMethodTranslationRepository
{
    public async Task<PaymentMethodTranslationEntity?> GetAsync(Guid paymentMethodId, string languageCode)
    {
        const string sql = "SELECT * FROM sys.payment_method_translations WHERE payment_method_id = @id AND language_code = @lang";
        var parameters = new Dictionary<string, object>
        {
            {"@id", paymentMethodId},
            {"@lang", languageCode}
        };
        var result = await DbManager.ReadAsync<PaymentMethodTranslationEntity>(sql, parameters, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<PaymentMethodTranslationEntity> UpsertAsync(PaymentMethodTranslationEntity entity, DbConnection? connection = null, DbTransaction? transaction = null)
    {
        const string sql = @"INSERT INTO sys.payment_method_translations (payment_method_id, language_code, name, description)
                             VALUES (@id, @lang, @name, @description)
                             ON CONFLICT (payment_method_id, language_code)
                             DO UPDATE SET name = EXCLUDED.name, description = EXCLUDED.description
                             RETURNING *";
        var parameters = new Dictionary<string, object>
        {
            {"@id", entity.PaymentMethodId},
            {"@lang", entity.LanguageCode},
            {"@name", entity.Name},
            {"@description", entity.Description}
        };
        var data = connection != null && transaction != null
            ? await DbManager.ReadAsync<PaymentMethodTranslationEntity>(sql, parameters, connection, transaction, GlobalSchema.Name)
            : await DbManager.ReadAsync<PaymentMethodTranslationEntity>(sql, parameters, GlobalSchema.Name);
        return data.First();
    }
}
