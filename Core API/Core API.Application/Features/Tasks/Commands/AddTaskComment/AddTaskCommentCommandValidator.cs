using FluentValidation;

namespace Core_API.Application.Features.Tasks.Commands.AddTaskComment
{

    public class AddTaskCommentCommandValidator : AbstractValidator<AddTaskCommentCommand>
    {
        public AddTaskCommentCommandValidator()
        {
            RuleFor(x => x.Context).NotNull();
            RuleFor(x => x.TaskId).GreaterThan(0);

            RuleFor(x => x.CommentDto.Comment)
                .NotEmpty().WithMessage("Comment text is required.")
                .MaximumLength(2000).WithMessage("Comment cannot exceed 2000 characters.");
        }
    }
}