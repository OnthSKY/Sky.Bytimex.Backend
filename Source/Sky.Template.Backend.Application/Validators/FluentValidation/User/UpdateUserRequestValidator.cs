using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.User;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(SharedResourceKeys.UserIdCannotBeEmpty);
        RuleFor(x => x.FirstName).NotEmpty().WithMessage(SharedResourceKeys.FirstNameCannotBeEmpty);
        RuleFor(x => x.LastName).NotEmpty().WithMessage(SharedResourceKeys.LastNameCannotBeEmpty);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(SharedResourceKeys.InvalidEmail);
        RuleFor(x => x.Status).NotEmpty().WithMessage(SharedResourceKeys.StatusIsRequired);
    }
}
