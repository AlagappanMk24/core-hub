namespace Core_API.Application.DTOs.Authentication.Request;

public class ValidateOtpDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string OtpToken { get; set; } = string.Empty;
    public string OtpIdentifier { get; set; } = string.Empty;
}

