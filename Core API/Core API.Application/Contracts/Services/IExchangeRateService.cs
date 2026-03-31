namespace Core_API.Application.Contracts.Services
{
    public interface IExchangeRateService
    {
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
    }
}