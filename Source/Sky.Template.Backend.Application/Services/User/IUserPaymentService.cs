using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Responses.PaymentResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserPaymentService
{
    [HasPermission(Permissions.Payments.Read)]
    Task<List<PaymentDto>> GetMyPaymentsAsync();
}

public class UserPaymentService : IUserPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserPaymentService(IPaymentRepository paymentRepository, IHttpContextAccessor httpContextAccessor)
    {
        _paymentRepository = paymentRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetUserId() => _httpContextAccessor.HttpContext.GetUserId();

    [HasPermission(Permissions.Payments.Read)]
    public async Task<List<PaymentDto>> GetMyPaymentsAsync()
    {
        var userId = GetUserId();
        var payments = await _paymentRepository.GetByBuyerIdAsync(userId);
        return payments.Select(p => new PaymentDto
        {
            PaymentId = p.Id,
            PaymentMethod = p.PaymentType,
            Amount = p.Amount,
            Status = p.PaymentStatus,
            CreatedAt = p.CreatedAt
        }).ToList();
    }
}

