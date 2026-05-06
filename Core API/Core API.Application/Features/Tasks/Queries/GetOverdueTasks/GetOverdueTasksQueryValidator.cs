using FluentValidation;

namespace Core_API.Application.Features.Tasks.Queries.GetOverdueTasks
{
    public class GetOverdueTasksQueryValidator : AbstractValidator<GetOverdueTasksQuery>
    {
        public GetOverdueTasksQueryValidator()
        {
            RuleFor(x => x.Context)
                .NotNull().WithMessage("Operation context is required");
        }
    }
}