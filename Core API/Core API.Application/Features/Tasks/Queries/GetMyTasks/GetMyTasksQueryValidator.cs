using FluentValidation;

namespace Core_API.Application.Features.Tasks.Queries.GetMyTasks
{
    public class GetMyTasksQueryValidator : AbstractValidator<GetMyTasksQuery>
    {
        public GetMyTasksQueryValidator()
        {
            RuleFor(x => x.Context)
                .NotNull().WithMessage("Operation context is required.");

            RuleFor(x => x.Filter)
                .ChildRules(filter =>
                {
                    filter.RuleFor(f => f.Page)
                        .GreaterThan(0).WithMessage("Page must be greater than 0.");

                    filter.RuleFor(f => f.PageSize)
                        .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
                });
        }
    }
}