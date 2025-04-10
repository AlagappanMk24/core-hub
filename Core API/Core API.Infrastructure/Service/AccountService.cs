using Core_API.Application.Contracts.DTOs.Request;
using Core_API.Application.Contracts.DTOs.Response;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Domain.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Core_API.Infrastructure.Service
{
    public class AccountService(IUnitOfWork unitOfWork) : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async Task<ResponseDto> ChangePasswordAsync(PasswordSettingsDto pwdSettingsDto)
        {
            var user = await _unitOfWork.Users.FindByEmailAsync(pwdSettingsDto.Email);

            if (user == null || !await _unitOfWork.Users.CheckPasswordAsync(user, pwdSettingsDto.CurrentPassword))
            {
                return new ResponseDto
                {
                    Message = "Email or current password is INCORRECT!",
                    IsSucceeded = false,
                    StatusCode = 400,
                };
            }
            var result = await _unitOfWork.Users.ChangePasswordAsync(user, pwdSettingsDto.CurrentPassword, pwdSettingsDto.NewPassword);
            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    Message = "Failed to change password, try again!",
                    IsSucceeded = false,
                    StatusCode = 400,
                };
            }
            return new ResponseDto
            {
                Message = "Your Password Changed Successfully.",
                IsSucceeded = true,
                StatusCode = 200,
            };
        }
        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string provider, string providerKey)
        {
            var result = await _unitOfWork.Users.CreateUserAsync(user);
            if (result.Succeeded)
            {
                var info = new UserLoginInfo(provider, providerKey, provider);
                await _unitOfWork.Users.AddLoginAsync(user, info);
            }
            return result;
        }
        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.FindByEmailAsync(email);
        }
        public async Task<ResponseDto> DeleteAccountAsync(LoginDto dto)
        {
            var user = await _unitOfWork.Users.FindByEmailAsync(dto.Email);
            if (user == null || !await _unitOfWork.Users.CheckPasswordAsync(user, dto.Password))
            {
                return new ResponseDto
                {
                    Message = "Email or current password is INCORRECT!",
                    IsSucceeded = false,
                    StatusCode = 400,
                };
            }

            var result = await _unitOfWork.Users.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    Message = "Failed to delete your account, try again later!",
                    IsSucceeded = false,
                    StatusCode = 400,
                };
            }
            return new ResponseDto
            {
                Message = "Your Account deleted Successfully,we hope you will back again.",
                IsSucceeded = true,
                StatusCode = 200,
            };
        }
        public async Task<ResponseDto> UpdateProfile(UserDto dto, string currentEmail)
        {
            var user = await _unitOfWork.Users.FindByEmailAsync(currentEmail);
            if (user == null)
            {
                return new ResponseDto
                {
                    Message = "An error occured, user not found!",
                    IsSucceeded = false,
                    StatusCode = 400,
                };
            }

            try
            {
                //user = _mapper.Map<ApplicationUser>(dto);
                user.FullName = dto.FullName;
                user.PhoneNumber = dto.PhoneNumber;
                user.Email = dto.Email;
                user.StreetAddress = dto.StreetAddress;
                user.City = dto.City;
                user.State = dto.State;
                user.PostalCode = dto.PostalCode;

                await _unitOfWork.Users.UpdateAsync(user);
                return new ResponseDto
                {
                    Message = "Updated your profile successfully.",
                    IsSucceeded = true,
                    StatusCode = 200,
                };
            }
            catch
            {
                return new ResponseDto
                {
                    Message = "Cannot update or edit your profile, try again!",
                    IsSucceeded = false,
                    StatusCode = 400,
                };
            }
        }
    }
}