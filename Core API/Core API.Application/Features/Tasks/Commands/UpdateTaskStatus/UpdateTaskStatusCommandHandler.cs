using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Tasks.Requests;
using Core_API.Application.DTOs.Tasks.Responses;
using Core_API.Application.Features.Tasks.Commands.UpdateTask;
using MediatR;
namespace Core_API.Application.Features.Tasks.Commands.UpdateTaskStatus
{
    public class UpdateTaskStatusCommandHandler(
         IRequestHandler<UpdateTaskCommand, OperationResult<TaskDto>> updateTaskHandler)
         : IRequestHandler<UpdateTaskStatusCommand, OperationResult<TaskDto>>
    {
        public async Task<OperationResult<TaskDto>> Handle(
            UpdateTaskStatusCommand request, CancellationToken cancellationToken)
        {
            var updateDto = new UpdateTaskDto { Status = request.Status };
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