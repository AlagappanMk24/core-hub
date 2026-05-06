using FluentValidation;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskComments
{
    public class GetTaskCommentsQueryValidator : AbstractValidator<GetTaskCommentsQuery>
    {
        public GetTaskCommentsQueryValidator()
        {
            RuleFor(x => x.TaskId)
                .GreaterThan(0).WithMessage("Valid task ID is required");
        }
    }
}