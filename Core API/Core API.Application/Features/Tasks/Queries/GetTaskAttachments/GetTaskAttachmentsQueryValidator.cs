using FluentValidation;

namespace Core_API.Application.Features.Tasks.Queries.GetTaskAttachments
{
    public class GetTaskAttachmentsQueryValidator : AbstractValidator<GetTaskAttachmentsQuery>
    {
        public GetTaskAttachmentsQueryValidator()
        {
            RuleFor(x => x.TaskId)
                .GreaterThan(0).WithMessage("Valid task ID is required");
        }
    }
}