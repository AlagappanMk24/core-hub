﻿using Core_API.Application.Contracts.DTOs.Request;
using Core_API.Application.Contracts.DTOs.Response;
using Core_API.Domain.Entities.Identity;

namespace Core_API.Application.Contracts.Service
{
    public interface IAuthService
    {
        Task<ResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<ResponseDto> LoginAsync(LoginDto loginDto);
        Task<ResponseDto> ValidateOtpAsync(ValidateOtpDto dto);
        Task<ResponseDto> ResendOtpAsync(ResendOtpDto dto);
        Task<ResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
        string GetExternalLoginUrl(string provider);
        Task<string> ExchangeAuthCodeForTokenAsync(ExternalLoginDto model);
        string GenerateJwtToken(ApplicationUser user);
    }
}
