using FluentValidation;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.User;

public class UserIdRequestValidator : AbstractValidator<UserIdRequest>
{
    public UserIdRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage(SharedResourceKeys.InvalidUserId);
    }
}

