using Core_API.Application.DTOs.Cart.Response;

namespace Core_API.Application.Contracts.Services
{
    public interface ICartService
    {
        Task<CartResponse> GetCartAsync(string userId);
        Task<CartResponse> AddToCartAsync(string userId, AddToCartRequest request);
        Task<CartResponse> UpdateCartItemCountAsync(string userId, int cartItemId, int count);
        Task<CartResponse> RemoveFromCartAsync(string userId, int cartItemId);
    }
}
