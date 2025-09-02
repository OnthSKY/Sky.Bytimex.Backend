using System;

namespace Sky.Template.Backend.Application.Services.Payments;

public class PaymentContext
{
    public Guid PaymentId { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

