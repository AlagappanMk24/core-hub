using FluentValidation;

namespace Core_API.Application.Features.CompanyRequests.Commands.UpdateCompany
{
    public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
    {
        public UpdateCompanyCommandValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Valid CompanyId is required.");
        }
    }
}