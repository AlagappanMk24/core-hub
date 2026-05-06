namespace Core_API.Application.Contracts.Services.Auth
{
    public interface IExternalAuthUrlBuilder
    {
        Task<string> BuildAuthorizationUrlAsync(string provider);
    }
}