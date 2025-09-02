using FluentValidation;
using Sky.Template.Backend.Contract.Requests.OrderStatusLogs;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.OrderStatusLogs;

public class CreateOrderStatusLogRequestValidator : AbstractValidator<CreateOrderStatusLogRequest>
{
    public CreateOrderStatusLogRequestValidator(IOrderRepository orderRepository)
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .MustAsync(async (id, cancellation) => await orderRepository.GetByIdAsync(id) != null)
            .WithMessage(SharedResourceKeys.OrderNotFound);
        RuleFor(x => x.OldStatus).NotEmpty().WithMessage(SharedResourceKeys.Required);
        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage(SharedResourceKeys.Required)
            .NotEqual(x => x.OldStatus).WithMessage(SharedResourceKeys.InvalidStatus);
        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage(SharedResourceKeys.MaxLength)
            .When(x => !string.IsNullOrEmpty(x.Note));
    }
}
