using Core_API.Application.Common.Base;

namespace Core_API.Application.Features.CompanyRequests.Commands.CreateCompanyRequest
{
    public record CreateCompanyRequestCommand : BaseCommand<CreateCompanyRequestResponse>
    {
        public string Email { get; init; } = string.Empty;
        public string CompanyName { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
    }

    public class CreateCompanyRequestResponse
    {
        public int? RequestId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}