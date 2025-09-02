using FluentValidation;
using Sky.Template.Backend.Contract.Requests.ErrorLogs;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.ErrorLog;

public class CreateErrorLogRequestValidator : AbstractValidator<CreateErrorLogRequest>
{
    public CreateErrorLogRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Path).MaximumLength(200);
        RuleFor(x => x.Method).MaximumLength(10);
    }
}
