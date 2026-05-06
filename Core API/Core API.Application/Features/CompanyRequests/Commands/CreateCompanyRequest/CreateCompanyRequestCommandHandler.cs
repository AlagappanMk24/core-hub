using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Email;
using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.CompanyRequests.Commands.CreateCompanyRequest
{
    public class CreateCompanyRequestCommandHandler(
         IUnitOfWork unitOfWork,
         IEmailServiceProvider emailServiceProvider,
         ILogger<CreateCompanyRequestCommandHandler> logger)
         : IRequestHandler<CreateCompanyRequestCommand, OperationResult<CreateCompanyRequestResponse>>
    {
        public async Task<OperationResult<CreateCompanyRequestResponse>> Handle(
            CreateCompanyRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (await unitOfWork.Companies.ExistsAsync(c => c.Name == request.CompanyName))
                    return OperationResult<CreateCompanyRequestResponse>.FailureResult("This company already exists in our system");

                if (await unitOfWork.CompanyRequests.HasPendingRequestAsync(request.Email, request.CompanyName))
                    return OperationResult<CreateCompanyRequestResponse>.FailureResult("You already have a pending request for this company");

                var companyRequest = new CompanyRequest
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    CompanyName = request.CompanyName,
                    RequestedAt = DateTime.UtcNow,
                    Status = CompanyRequestStatus.Pending,
                    RequestToken = Guid.NewGuid().ToString("N")
                };

                await unitOfWork.CompanyRequests.AddAsync(companyRequest);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                await emailServiceProvider.SendCompanyRequestToAdminAsync(companyRequest);

                logger.LogInformation("Company request created successfully. RequestId: {RequestId}", companyRequest.Id);
                return OperationResult<CreateCompanyRequestResponse>.SuccessResult(new CreateCompanyRequestResponse
                {
                    RequestId = companyRequest.Id,
                    Message = "Your request has been submitted successfully. You will receive an email once it's processed."
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating company request for email {Email}", request.Email);
                return OperationResult<CreateCompanyRequestResponse>.FailureResult("Failed to process company request");
            }
        }
    }
}
