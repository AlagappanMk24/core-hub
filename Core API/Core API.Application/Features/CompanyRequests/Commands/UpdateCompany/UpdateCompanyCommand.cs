using Core_API.Application.Common.Base;

namespace Core_API.Application.Features.CompanyRequests.Commands.UpdateCompany
{
    /// <summary>
    /// Command to update the authenticated user's company association
    /// </summary>
    public record UpdateCompanyCommand : BaseCommand<UpdateCompanyResponse>
    {
        public int CompanyId { get; init; }
    }

    public class UpdateCompanyResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}