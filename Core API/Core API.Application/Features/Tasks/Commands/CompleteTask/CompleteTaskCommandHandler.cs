using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Application.Features.Tasks.Commands.UpdateTaskStatus;
using MediatR;

namespace Core_API.Application.Features.Tasks.Commands.CompleteTask
{
    public class CompleteTaskCommandHandler(
         IRequestHandler<UpdateTaskStatusCommand, OperationResult<TaskDto>> updateStatusHandler)
         : IRequestHandler<CompleteTaskCommand, OperationResult<bool>>
    {
        public async Task<OperationResult<bool>> Handle(
            CompleteTaskCommand request, CancellationToken cancellationToken)
        {
            var statusCommand = new UpdateTaskStatusCommand
            {
                TaskId = request.TaskId,
                Status = Domain.Enums.TaskStatus.Completed,
                Context = request.Context
            };

            var result = await updateStatusHandler.Handle(statusCommand, cancellationToken);
            return OperationResult<bool>.SuccessResult(result.IsSuccess);
        }
    }
}