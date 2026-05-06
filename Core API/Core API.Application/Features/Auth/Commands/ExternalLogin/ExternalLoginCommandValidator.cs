using FluentValidation;

namespace Core_API.Application.Features.Auth.Commands.ExternalLogin
{
    public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
    {
        public ExternalLoginCommandValidator()
        {
            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("Provider is required")
                .Must(p => new[] { "google", "microsoft", "facebook", "github" }.Contains(p.ToLower()))
                .WithMessage("Provider must be one of: google, microsoft, facebook, github");

            RuleFor(x => x.AuthorizationCode)
                .NotEmpty().WithMessage("Authorization code is required");
        }
    }
}