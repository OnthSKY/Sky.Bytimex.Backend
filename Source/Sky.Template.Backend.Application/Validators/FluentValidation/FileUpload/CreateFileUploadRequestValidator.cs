using FluentValidation;
using Sky.Template.Backend.Contract.Requests.FileUploads;

namespace Sky.Template.Backend.Application.Validators.FluentValidation.FileUpload;

public class CreateFileUploadRequestValidator : AbstractValidator<CreateFileUploadRequest>
{
    public CreateFileUploadRequestValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.FileExtension).NotEmpty().MaximumLength(10);
        RuleFor(x => x.FileSize).GreaterThan(0);
        RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.FileType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
    }
}

