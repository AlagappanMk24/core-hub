using Core_API.Application.DTOs.Product.Response;

namespace Core_API.Application.Contracts.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
        Task<IEnumerable<ProductResponse>> GetTrendingProductsAsync();
    }
}
