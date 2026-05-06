using FluentValidation;

namespace Core_API.Application.Features.Tasks.Queries.GetTasksDueToday
{
    public class GetTasksDueTodayQueryValidator : AbstractValidator<GetTasksDueTodayQuery>
    {
        public GetTasksDueTodayQueryValidator()
        {
            RuleFor(x => x.Context)
                .NotNull().WithMessage("Operation context is required");
        }
    }
}