using FluentValidation;

namespace Core_API.Application.Features.Auth.Commands.ValidateOtp
{
    /// <summary>
    /// Validator for ValidateOtpCommand
    /// </summary>
    public class ValidateOtpCommandValidator : AbstractValidator<ValidateOtpCommand>
    {
        public ValidateOtpCommandValidator()
        {
            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required.")
                .Length(6).WithMessage("OTP must be 6 digits.")
                .Matches(@"^\d{6}$").WithMessage("OTP must contain only numbers.");

            RuleFor(x => x.OtpToken)
                .NotEmpty().WithMessage("OTP Token is required.");

            RuleFor(x => x.OtpIdentifier)
                .NotEmpty().WithMessage("OTP Identifier is required.");
        }
    }
}
