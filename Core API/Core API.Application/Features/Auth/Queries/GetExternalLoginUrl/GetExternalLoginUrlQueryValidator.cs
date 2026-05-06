using FluentValidation;

namespace Core_API.Application.Features.Auth.Queries.GetExternalLoginUrl
{
    /// <summary>
    /// Validator for GetExternalLoginUrlQuery
    /// </summary>
    public class GetExternalLoginUrlQueryValidator : AbstractValidator<GetExternalLoginUrlQuery>
    {
        public GetExternalLoginUrlQueryValidator()
        {
            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("Provider is required.")
                .Must(provider => IsSupportedProvider(provider))
                .WithMessage("Unsupported authentication provider.");
        }

        private static bool IsSupportedProvider(string provider)
        {
            return provider.ToLowerInvariant() is "google" or "microsoft" or "facebook" or "github";
        }
    }
}