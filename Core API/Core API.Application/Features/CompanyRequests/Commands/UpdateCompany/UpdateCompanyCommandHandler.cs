using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.CompanyRequests.Commands.UpdateCompany
{
    public class UpdateCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        ILogger<UpdateCompanyCommandHandler> logger)
        : IRequestHandler<UpdateCompanyCommand, OperationResult<UpdateCompanyResponse>>
    {
        public async Task<OperationResult<UpdateCompanyResponse>> Handle(
            UpdateCompanyCommand request,
            CancellationToken cancellationToken)
        {
            var context = request.Context;

            try
            {
                var user = await userManager.FindByIdAsync(context.UserId);
                if (user == null)
                    return OperationResult<UpdateCompanyResponse>.FailureResult("User not found");

                var company = await unitOfWork.Companies.GetByIdAsync(request.CompanyId);
                if (company == null)
                    return OperationResult<UpdateCompanyResponse>.FailureResult("Company not found");

                // Security Check
                if (!context.CanAccessCompany(request.CompanyId) && !context.IsSuperAdmin)
                    return OperationResult<UpdateCompanyResponse>.FailureResult("You do not have permission to switch to this company");

                user.CompanyId = request.CompanyId;
                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                    return OperationResult<UpdateCompanyResponse>.FailureResult(
                        $"Failed to update company: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");

                // Generate new JWT with updated company context
                var newToken = await jwtService.GenerateJwtTokenAsync(user);

                logger.LogInformation("Company updated successfully for user {UserId} to CompanyId {CompanyId}",
                    context.UserId, request.CompanyId);

                return OperationResult<UpdateCompanyResponse>.SuccessResult(new UpdateCompanyResponse
                {
                    Token = newToken,
                    Message = "Company updated successfully"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating company for user {UserId}", context.UserId);
                return OperationResult<UpdateCompanyResponse>.FailureResult("An unexpected error occurred while updating company");
            }
        }
    }
}