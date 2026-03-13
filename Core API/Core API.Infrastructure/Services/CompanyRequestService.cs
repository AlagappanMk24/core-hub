using AutoMapper;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Authentication.Request.CompanyRequest;
using Core_API.Domain.Entities;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Enums;
using Core_API.Domain.Models.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Core_API.Infrastructure.Services
{
    public class CompanyRequestService(
        IUnitOfWork unitOfWork,
        IEmailSendingService emailService,
        UserManager<ApplicationUser> userManager,
        ILogger<CompanyRequestService> logger,
        IMapper mapper,
        IConfiguration configuration) : ICompanyRequestService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmailSendingService _emailService = emailService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<CompanyRequestService> _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IConfiguration _configuration = configuration;

        #region Public Endpoints

        public async Task<(bool Success, string Message, int? RequestId)> CreateRequestAsync(CreateCompanyRequestDto dto)
        {
            try
            {
                // Check if company already exists
                var companyExists = await _unitOfWork.Companies.ExistsAsync(c => c.Name == dto.CompanyName);
                if (companyExists)
                {
                    return (false, "This company already exists in our system", null);
                }

                // Check for pending request
                var hasPendingRequest = await _unitOfWork.CompanyRequests.HasPendingRequestAsync(dto.Email, dto.CompanyName);
                if (hasPendingRequest)
                {
                    return (false, "You already have a pending request for this company", null);
                }

                // Create new request
                var request = new CompanyRequest
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    CompanyName = dto.CompanyName,
                    RequestedAt = DateTime.UtcNow,
                    Status = CompanyRequestStatus.Pending,
                    RequestToken = Guid.NewGuid().ToString("N")
                };

                await _unitOfWork.CompanyRequests.AddAsync(request);
                await _unitOfWork.SaveChangesAsync();

                // Send email to admin
                await SendRequestToAdmin(request);

                _logger.LogInformation("Company request created successfully. RequestId: {RequestId}", request.Id);

                return (true, "Your request has been submitted successfully. You will receive an email once it's processed.", request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company request for {Email}", dto.Email);
                return (false, "Failed to process company request. Please try again later.", null);
            }
        }

        public async Task<List<RequestStatusResponseDto>> GetRequestStatusAsync(string email)
        {
            var requests = await _unitOfWork.CompanyRequests.GetRequestsByEmailAsync(email);

            return requests.Select(r => new RequestStatusResponseDto
            {
                Id = r.Id,
                CompanyName = r.CompanyName,
                RequestedAt = r.RequestedAt,
                Status = r.Status.ToString(),
                ProcessedAt = r.ProcessedAt,
                RejectionReason = r.RejectionReason
            }).ToList();
        }

        #endregion

        #region Admin Endpoints

        public async Task<CompanyRequestListResponseDto> GetPagedRequestsAsync(int page, int pageSize, string status, string search)
        {
            var (requests, totalCount, pendingCount, approvedCount, rejectedCount) =
                await _unitOfWork.CompanyRequests.GetPagedRequestsAsync(page, pageSize, status, search);

            return new CompanyRequestListResponseDto
            {
                Requests = requests.Select(r => new CompanyRequestResponseDto
                {
                    Id = r.Id,
                    FullName = r.FullName,
                    Email = r.Email,
                    CompanyName = r.CompanyName,
                    RequestedAt = r.RequestedAt,
                    Status = r.Status.ToString(),
                    ProcessedAt = r.ProcessedAt,
                    ProcessedBy = r.ProcessedBy,
                    RejectionReason = r.RejectionReason
                }).ToList(),
                TotalCount = totalCount,
                PendingCount = pendingCount,
                ApprovedCount = approvedCount,
                RejectedCount = rejectedCount
            };
        }

        public async Task<CompanyRequestResponseDto> GetRequestByIdAsync(int id)
        {
            var request = await _unitOfWork.CompanyRequests.GetRequestByIdAsync(id);

            if (request == null)
                return null;

            return new CompanyRequestResponseDto
            {
                Id = request.Id,
                FullName = request.FullName,
                Email = request.Email,
                CompanyName = request.CompanyName,
                RequestedAt = request.RequestedAt,
                Status = request.Status.ToString(),
                ProcessedAt = request.ProcessedAt,
                ProcessedBy = request.ProcessedBy,
                RejectionReason = request.RejectionReason
            };
        }

        public async Task<(bool Success, string Message, int? CompanyId, string UserId)> ApproveRequestAsync(int id, string adminId)
        {
            var request = await _unitOfWork.CompanyRequests.GetRequestByIdAsync(id);

            if (request == null)
                return (false, "Company request not found", null, null);

            if (request.Status != CompanyRequestStatus.Pending)
                return (false, $"Request is already {request.Status.ToString().ToLower()}", null, null);

            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
                return (false, "Admin not found", null, null);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Create company
                var company = new Company
                {
                    Name = request.CompanyName,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = admin.Email
                };

                await _unitOfWork.Companies.AddAsync(company);
                await _unitOfWork.SaveChangesAsync();

                // Update request status
                request.Status = CompanyRequestStatus.Approved;
                request.ProcessedAt = DateTime.UtcNow;
                request.ProcessedBy = admin.Email;

                _unitOfWork.CompanyRequests.Update(request);
                await _unitOfWork.SaveChangesAsync();

                // Find or create user
                var user = await _userManager.FindByEmailAsync(request.Email);
                string userId;

                if (user == null)
                {
                    // Create new user
                    user = new ApplicationUser
                    {
                        UserName = request.Email,
                        Email = request.Email,
                        FullName = request.FullName,
                        CompanyId = company.Id,
                        EmailConfirmed = true,
                    };

                    var tempPassword = GenerateTemporaryPassword();
                    var createResult = await _userManager.CreateAsync(user, tempPassword);

                    if (!createResult.Succeeded)
                    {
                        throw new Exception($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }

                    await _userManager.AddToRoleAsync(user, "Customer");
                    userId = user.Id;

                    // Send welcome email with temp password
                    await _emailService.SendWelcomeEmailAsync(new WelcomeEmailRequest
                    {
                        Email = user.Email,
                        Name = user.FullName,
                        TemporaryPassword = tempPassword,
                        HtmlMessage = "http://localhost:4200/auth/login"
                    });
                }
                else
                {
                    // Update existing user
                    user.CompanyId = company.Id;
                    await _userManager.UpdateAsync(user);
                    userId = user.Id;

                    // Send notification email
                    var loginLink = "http://localhost:4200/auth/login";
                    var emailBody = EmailTemplates.GetCompanyApprovedEmail(
                        user.FullName,
                        company.Name,
                        loginLink
                    );

                    await _emailService.SendEmailAsync(new EmailRequest
                    {
                        Email = user.Email,
                        Subject = "Your Company Registration has been Approved!",
                        HtmlMessage = emailBody
                    });
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Company request {RequestId} approved successfully", id);

                return (true, "Company request approved successfully", company.Id, userId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error approving company request {RequestId}", id);
                return (false, "Failed to approve company request", null, null);
            }
        }

        public async Task<(bool Success, string Message)> RejectRequestAsync(int id, string reason, string adminId)
        {
            var request = await _unitOfWork.CompanyRequests.GetRequestByIdAsync(id);

            if (request == null)
                return (false, "Company request not found");

            if (request.Status != CompanyRequestStatus.Pending)
                return (false, $"Request is already {request.Status.ToString().ToLower()}");

            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
                return (false, "Admin not found");

            try
            {
                // Update request
                request.Status = CompanyRequestStatus.Rejected;
                request.ProcessedAt = DateTime.UtcNow;
                request.ProcessedBy = admin.Email;
                request.RejectionReason = reason;

                _unitOfWork.CompanyRequests.Update(request);
                await _unitOfWork.SaveChangesAsync();

                // Send rejection email
                var supportLink = "http://localhost:4200/support";
                var emailBody = EmailTemplates.GetCompanyRejectedEmail(
                    request.FullName,
                    request.CompanyName,
                    reason,
                    supportLink
                );

                await _emailService.SendEmailAsync(new EmailRequest
                {
                    Email = request.Email,
                    Subject = "Update on Your Company Registration Request",
                    HtmlMessage = emailBody
                });

                _logger.LogInformation("Company request {RequestId} rejected successfully", id);

                return (true, "Company request rejected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting company request {RequestId}", id);
                return (false, "Failed to reject company request");
            }
        }

        #endregion

        #region User Company Update

        public async Task<(bool Success, string Message, string Token)> UpdateUserCompanyAsync(string userId, int companyId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found", null);
                }

                var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
                if (company == null)
                {
                    return (false, "Company not found", null);
                }

                user.CompanyId = companyId;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);
                }

                // Generate new JWT with updated CompanyId
                var token = await GenerateJwtToken(user);

                return (true, "Company updated successfully", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company for user {UserId}", userId);
                return (false, "Failed to update company", null);
            }
        }

        #endregion

        #region Private Methods

        private async Task SendRequestToAdmin(CompanyRequest request)
        {
            var adminEmail = _configuration["AdminSettings:Email"] ?? "alagappanmuthukumar1998@gmail.com";
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:4200";

            var reviewLink = $"{baseUrl}/company-requests/{request.Id}";
            var allRequestsLink = $"{baseUrl}/company-requests";

            var emailBody = EmailTemplates.GetCompanyRequestAdminEmail(
                "Administrator",
                request,
                reviewLink
            );

            await _emailService.SendEmailAsync(new EmailRequest
            {
                Email = adminEmail,
                Subject = $"New Company Registration Request: {request.CompanyName}",
                HtmlMessage = emailBody
            });

            _logger.LogInformation("Company request email sent to admin for request {RequestId}", request.Id);
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("uid", user.Id),
                new("companyId", user.CompanyId?.ToString() ?? "0"),
                new("customerId", user.CustomerId?.ToString() ?? "0")
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:ValidIssuer"],
                audience: _configuration["JwtSettings:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Ensure it meets complexity requirements
            if (!password.Any(char.IsUpper))
                password = "A" + password.Substring(1);
            if (!password.Any(char.IsLower))
                password = password.Substring(0, 5) + "a" + password.Substring(6);
            if (!password.Any(char.IsDigit))
                password = password.Substring(0, 8) + "1" + password.Substring(9);
            if (!password.Any(ch => "!@#$%".Contains(ch)))
                password = string.Concat(password.AsSpan(0, 10), "!", password.AsSpan(11));

            return password;
        }
        #endregion
    }
}