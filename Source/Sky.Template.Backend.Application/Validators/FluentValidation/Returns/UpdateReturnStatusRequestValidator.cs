using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Returns;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.Returns;

public class UpdateReturnStatusRequestValidator : AbstractValidator<UpdateReturnStatusRequest>
{
    public UpdateReturnStatusRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty();
    }
}
