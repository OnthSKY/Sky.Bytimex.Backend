using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Returns;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Returns;

public class CreateReturnRequestValidator : AbstractValidator<CreateReturnRequest>
{
    public CreateReturnRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.BuyerId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
    }
}
