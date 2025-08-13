namespace Core_API.Infrastructure.Services
{
    //public class CartService(CoreAPIDbContext context) : ICartService
    //{
    //    private readonly CoreAPIDbContext _context = context;
    //    public async Task<CartViewModel> GetCartAsync(string userId)
    //    {
    //        var cartItems = await _context.ShoppingCarts
    //            .Where(sc => sc.ApplicationUserId == userId && sc.IsDeleted == false)
    //            .Include(sc => sc.Product)
    //            .ThenInclude(p => p.ProductImages)
    //            .ToListAsync();

    //        var cartViewModel = new CartViewModel
    //        {
    //            Items = cartItems.Select(sc => new CartItemViewModel
    //            {
    //                Id = sc.Id,
    //                ProductId = sc.ProductId,
    //                ProductName = sc.Product.Title,
    //                Price = sc.Product.Price,
    //                DiscountPrice = sc.Product.IsDiscounted ? sc.Product.DiscountPrice : null,
    //                Count = sc.Count,
    //                ImageUrl = sc.Product.ProductImages.FirstOrDefault()?.ImageUrl ?? null
    //            }).ToList()
    //        };

    //        cartViewModel.TotalPrice = cartViewModel.Items.Sum(i => (i.DiscountPrice ?? i.Price) * i.Count);
    //        return cartViewModel;
    //    }

    //    public async Task<CartViewModel> AddToCartAsync(string userId, AddToCartRequest request)
    //    {
    //        var product = await _context.Products.FindAsync(request.ProductId);
    //        if (product == null || !product.IsActive)
    //        {
    //            throw new System.Exception("Product not found or inactive.");
    //        }

    //        var cartItem = await _context.ShoppingCarts
    //            .FirstOrDefaultAsync(sc => sc.ApplicationUserId == userId && sc.ProductId == request.ProductId && sc.IsDeleted == false);

    //        if (cartItem != null)
    //        {
    //            cartItem.Count += request.Count;
    //            if (cartItem.Count > 1000)
    //            {
    //                throw new System.Exception("Quantity cannot exceed 1000.");
    //            }
    //        }
    //        else
    //        {
    //            cartItem = new ShoppingCart
    //            {
    //                ApplicationUserId = userId,
    //                ProductId = request.ProductId,
    //                Count = request.Count
    //            };
    //            _context.ShoppingCarts.Add(cartItem);
    //        }

    //        await _context.SaveChangesAsync();
    //        return await GetCartAsync(userId);
    //    }

    //    public async Task<CartViewModel> UpdateCartItemCountAsync(string userId, int cartItemId, int count)
    //    {
    //        var cartItem = await _context.ShoppingCarts
    //            .FirstOrDefaultAsync(sc => sc.ApplicationUserId == userId && sc.Id == cartItemId && sc.IsDeleted == false);

    //        if (cartItem == null)
    //        {
    //            throw new System.Exception("Cart item not found.");
    //        }

    //        if (count <= 0)
    //        {
    //            cartItem.IsDeleted = true;
    //        }
    //        else
    //        {
    //            if (count > 1000)
    //            {
    //                throw new System.Exception("Quantity cannot exceed 1000.");
    //            }
    //            cartItem.Count = count;
    //        }

    //        await _context.SaveChangesAsync();
    //        return await GetCartAsync(userId);
    //    }

    //    public async Task<CartViewModel> RemoveFromCartAsync(string userId, int cartItemId)
    //    {
    //        var cartItem = await _context.ShoppingCarts
    //            .FirstOrDefaultAsync(sc => sc.ApplicationUserId == userId && sc.Id == cartItemId && sc.IsDeleted == false);

    //        if (cartItem == null)
    //        {
    //            throw new System.Exception("Cart item not found.");
    //        }

    //        cartItem.IsDeleted = true;
    //        await _context.SaveChangesAsync();
    //        return await GetCartAsync(userId);
    //    }
    //}
}