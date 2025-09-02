using System.Text.Json.Serialization;

namespace Sky.Template.Backend.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethodType
{
    CARD,
    TRANSFER,
    CRYPTO,
    CASH
}
