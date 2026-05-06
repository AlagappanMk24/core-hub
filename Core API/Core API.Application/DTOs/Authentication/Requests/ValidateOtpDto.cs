namespace Core_API.Application.DTOs.Auth.Requests;

public class ValidateOtpDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string OtpToken { get; set; } = string.Empty;
    public string OtpIdentifier { get; set; } = string.Empty;
}

