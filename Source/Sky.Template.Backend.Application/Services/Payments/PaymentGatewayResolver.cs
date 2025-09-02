using System;
using System.Collections.Generic;
using System.Linq;
using Sky.Template.Backend.Core.Exceptions;

namespace Sky.Template.Backend.Application.Services.Payments;

public interface IPaymentGatewayResolver
{
    IPaymentGateway Resolve(string paymentMethod);
}

public class PaymentGatewayResolver : IPaymentGatewayResolver
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    public IPaymentGateway Resolve(string paymentMethod)
    {
        var gateway = _gateways.FirstOrDefault(g =>
            g.Method.Equals(paymentMethod, StringComparison.OrdinalIgnoreCase));
        if (gateway == null)
            throw new BusinessRulesException("PaymentMethodNotSupported", paymentMethod);
        return gateway;
    }
}

