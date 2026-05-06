using Core_API.Application.DTOs.Cart.Response;

namespace Core_API.Application.Contracts.Services.Cart
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(string userId);
        Task<CartResponseDto> AddToCartAsync(string userId, AddToCartRequest request);
        Task<CartResponseDto> UpdateCartItemCountAsync(string userId, int cartItemId, int count);
        Task<CartResponseDto> RemoveFromCartAsync(string userId, int cartItemId);
    }
}
