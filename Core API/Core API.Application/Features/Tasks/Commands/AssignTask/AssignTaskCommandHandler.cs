using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Application.Features.Tasks.Commands.UpdateTask;
using MediatR;

namespace Core_API.Application.Features.Tasks.Commands.AssignTask
{
    public class AssignTaskCommandHandler(
        IRequestHandler<UpdateTaskCommand, OperationResult<TaskDto>> updateTaskHandler)
        : IRequestHandler<AssignTaskCommand, OperationResult<TaskDto>>
    {
        public async Task<OperationResult<TaskDto>> Handle(
            AssignTaskCommand request, CancellationToken cancellationToken)
        {
            var updateDto = new UpdateTaskDto { AssignedToUserId = request.UserId };
            var updateCommand = new UpdateTaskCommand
            {
                TaskId = request.TaskId,
                UpdateDto = updateDto,
                Context = request.Context
            };

            return await updateTaskHandler.Handle(updateCommand, cancellationToken);
        }
    }
}