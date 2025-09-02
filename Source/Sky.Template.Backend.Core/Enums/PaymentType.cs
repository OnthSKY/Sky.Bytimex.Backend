using System.Text.Json.Serialization;

namespace Sky.Template.Backend.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentType
{
    CREDIT_CARD,
    BANK_TRANSFER,
    CRYPTO,
    CASH
}
