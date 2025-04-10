namespace Core_API.Application.Contracts.DTOs.Request
{
    public class ValidateOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}

