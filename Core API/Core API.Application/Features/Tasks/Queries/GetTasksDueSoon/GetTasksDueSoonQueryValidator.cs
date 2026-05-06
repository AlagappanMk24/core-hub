using FluentValidation;

namespace Core_API.Application.Features.Tasks.Queries.GetTasksDueSoon
{
    public class GetTasksDueSoonQueryValidator : AbstractValidator<GetTasksDueSoonQuery>
    {
        public GetTasksDueSoonQueryValidator()
        {
            RuleFor(x => x.Days)
                .GreaterThan(0).WithMessage("Days must be greater than 0")
                .LessThanOrEqualTo(30).WithMessage("Days cannot exceed 30");

            RuleFor(x => x.Context)
                .NotNull().WithMessage("Operation context is required");
        }
    }
}