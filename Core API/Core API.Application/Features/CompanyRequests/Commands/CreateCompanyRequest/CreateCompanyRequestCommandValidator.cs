using FluentValidation;

namespace Core_API.Application.Features.CompanyRequests.Commands.CreateCompanyRequest
{
    public class CreateCompanyRequestCommandValidator : AbstractValidator<CreateCompanyRequestCommand>
    {
        public CreateCompanyRequestCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(150);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        }
    }
}