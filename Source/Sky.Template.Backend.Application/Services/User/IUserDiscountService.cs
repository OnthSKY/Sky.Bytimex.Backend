using Sky.Template.Backend.Contract.Requests.Discounts;
using Sky.Template.Backend.Contract.Responses.DiscountResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserDiscountService
{
    [HasPermission(Permissions.Discounts.Apply)]
    Task<BaseControllerResponse<DiscountResultDto>> ApplyCouponAsync(Guid buyerId, ApplyCouponRequest request);
    [HasPermission(Permissions.Discounts.Apply)]
    Task<BaseControllerResponse<bool>> ValidateCouponAsync(string code, Guid buyerId);
}

public class UserDiscountService : IUserDiscountService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IDiscountUsageRepository _discountUsageRepository;

    public UserDiscountService(IDiscountRepository discountRepository, IDiscountUsageRepository discountUsageRepository)
    {
        _discountRepository = discountRepository;
        _discountUsageRepository = discountUsageRepository;
    }

    [HasPermission(Permissions.Discounts.Apply)]
    public async Task<BaseControllerResponse<DiscountResultDto>> ApplyCouponAsync(Guid buyerId, ApplyCouponRequest request)
    {
        var discount = await _discountRepository.GetByCodeAsync(request.CouponCode);
        if (discount == null)
        {
            return ControllerResponseBuilder.Success(new DiscountResultDto
            {
                IsValid = false,
                DiscountAmount = 0,
                Message = "CouponNotFound"
            });
        }

        var validationMessage = await ValidateInternalAsync(discount, buyerId, request.CartTotal);
        if (validationMessage != null)
        {
            return ControllerResponseBuilder.Success(new DiscountResultDto
            {
                IsValid = false,
                DiscountAmount = 0,
                Message = validationMessage
            });
        }

        var discountAmount = CalculateDiscount(discount, request.CartTotal);
        return ControllerResponseBuilder.Success(new DiscountResultDto
        {
            IsValid = true,
            DiscountAmount = discountAmount,
            Message = "CouponApplied"
        });
    }

    [HasPermission(Permissions.Discounts.Apply)]
    public async Task<BaseControllerResponse<bool>> ValidateCouponAsync(string code, Guid buyerId)
    {
        var discount = await _discountRepository.GetByCodeAsync(code);
        if (discount == null)
            return ControllerResponseBuilder.Success(false);

        var validationMessage = await ValidateInternalAsync(discount, buyerId, null);
        return ControllerResponseBuilder.Success(validationMessage == null);
    }

    private async Task<string?> ValidateInternalAsync(DiscountEntity discount, Guid buyerId, decimal? cartTotal)
    {
        var now = DateTime.UtcNow;
        if (discount.StartDate > now || discount.EndDate < now)
            return "CouponExpired";

        if (discount.UsageLimit.HasValue)
        {
            var usedCount = await _discountUsageRepository.CountByDiscountIdAsync(discount.Id);
            if (usedCount >= discount.UsageLimit.Value)
                return "CouponUsageLimitReached";
        }

        var buyerUsage = await _discountUsageRepository.CountByDiscountAndBuyerAsync(discount.Id, buyerId);
        if (buyerUsage > 0)
            return "CouponAlreadyUsed";

        if (cartTotal.HasValue && cartTotal.Value <= 0)
            return "CartTotalInvalid";

        return null;
    }

    private static decimal CalculateDiscount(DiscountEntity discount, decimal cartTotal)
    {
        if (!Enum.TryParse<DiscountType>(discount.DiscountType, true, out var type))
            return 0;

        var amount = type == DiscountType.PERCENTAGE
            ? cartTotal * discount.Value / 100m
            : discount.Value;

        if (amount > cartTotal)
            amount = cartTotal;
        return amount;
    }
}
