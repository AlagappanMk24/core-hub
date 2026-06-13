using FluentValidation;

namespace Core_API.Application.Features.Customers.Commands.CreateCustomer
{
    /// <summary>
    /// Validator for CreateCustomerCommand
    /// </summary>
    public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCustomerCommandValidator"/> class.
        /// </summary>
        public CreateCustomerCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

            RuleFor(x => x.AddressLine1)
                .NotEmpty().WithMessage("Address line 1 is required")
                .MaximumLength(200).WithMessage("Address line 1 cannot exceed 200 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("Country code is required")
                .Length(2).WithMessage("Country code must be 2 characters");

            RuleFor(x => x.ZipCode)
                .NotEmpty().WithMessage("Zip code is required")
                .MaximumLength(20).WithMessage("Zip code cannot exceed 20 characters");

            RuleFor(x => x.CreditLimit)
              .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be a positive number");

            RuleFor(x => x.DefaultCurrency)
                .Length(3).When(x => !string.IsNullOrEmpty(x.DefaultCurrency))
                .WithMessage("Currency code must be 3 characters");
        }
    }
}