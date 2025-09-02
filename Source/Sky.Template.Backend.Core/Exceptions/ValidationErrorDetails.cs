using FluentValidation.Results;

namespace Sky.Template.Backend.Core.Exceptions;


public class ValidationErrorDetails : ErrorDetails
{
    public IEnumerable<ValidationFailure> Errors { get; set; }
}
