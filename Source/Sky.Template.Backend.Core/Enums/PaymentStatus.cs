using System.Text.Json.Serialization;

namespace Sky.Template.Backend.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    AWAITING,
    PAID,
    CONFIRMED
}
