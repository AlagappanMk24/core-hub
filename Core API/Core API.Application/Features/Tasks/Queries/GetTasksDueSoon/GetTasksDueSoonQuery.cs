using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Tasks.Responses;

namespace Core_API.Application.Features.Tasks.Queries.GetTasksDueSoon
{
    public record GetTasksDueSoonQuery : BaseQuery<IEnumerable<TaskDto>>
    {
        public int Days { get; set; } = 3;
    }
}