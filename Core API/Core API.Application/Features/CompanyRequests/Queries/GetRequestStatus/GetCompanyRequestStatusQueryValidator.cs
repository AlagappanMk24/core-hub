using FluentValidation;

namespace Core_API.Application.Features.CompanyRequests.Queries.GetRequestStatus
{
    public class GetCompanyRequestStatusQueryValidator : AbstractValidator<GetCompanyRequestStatusQuery>
    {
        public GetCompanyRequestStatusQueryValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Valid email is required.");
        }
    }
}