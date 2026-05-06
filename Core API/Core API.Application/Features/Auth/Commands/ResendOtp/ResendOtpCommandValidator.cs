using FluentValidation;

namespace Core_API.Application.Features.Auth.Commands.ResendOtp
{
    /// <summary>
    /// Validator for Resend OTP Command
    /// </summary>
    public class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
    {
        public ResendOtpCommandValidator()
        {
            RuleFor(x => x.OtpIdentifier)
                .NotEmpty().WithMessage("OTP Identifier is required.")
                .MinimumLength(5).WithMessage("OTP Identifier is too short.")
                .MaximumLength(100).WithMessage("OTP Identifier is too long.");
        }
    }
}
