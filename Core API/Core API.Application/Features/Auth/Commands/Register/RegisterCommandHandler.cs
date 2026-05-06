using AutoMapper;
using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities.Identity;
using MediatR;
using Microsoft.Extensions.Logging;
using Core_API.Application.DTOs.Common;

namespace Core_API.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand,ResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<RegisterCommandHandler> _logger = logger;


        /// <summary>
        /// Registers a new user with the provided details.
        /// </summary>
        /// <param name="registerDto">The registration details, including email, password, and roles.</param>
        /// <returns>A <see cref="ResponseDto"/> indicating success or failure.</returns>
        public async Task<ResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if email is already registered
            if (await _unitOfWork.AuthUsers.FindByEmailAsync(request.Email) != null)
            {
                return new Core_API.Application.DTOs.Common.ResponseDto
                {
                    Message = $"Email '{request.Email}' is already taken.",
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            // Map DTO to ApplicationUser entity
            var user = _mapper.Map<ApplicationUser>(request);

            // Create user with password
            var result = await _unitOfWork.AuthUsers.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                // Aggregate identity errors into a single message
                var errors = string.Join(", \n", result.Errors.Select(e => e.Description));
                return new ResponseDto
                {
                    Message = errors,
                    IsSucceeded = false,
                    StatusCode = 400
                };
            }

            // Assign roles: default to "User" if none specified
            if (request.Roles is null || request.Roles.Count == 0)
            {
                await _unitOfWork.AuthUsers.AddToRoleAsync(user, "User");
            }
            else
            {
                foreach (var role in request.Roles)
                {
                    await _unitOfWork.AuthUsers.AddToRoleAsync(user, role);
                }
            }

            return new ResponseDto
            {
                Message = "Account created successfully.",
                IsSucceeded = true,
                StatusCode = 200
            };
        }
    }
}