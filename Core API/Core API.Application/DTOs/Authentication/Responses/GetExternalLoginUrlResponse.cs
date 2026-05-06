namespace Core_API.Application.DTOs.Authentication.Responses
{
    /// <summary>
    /// Response model for external login URL
    /// </summary>
    public class GetExternalLoginUrlResponse
    {
        public string RedirectUrl { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }
}