namespace Core_API.Infrastructure.Services
{
    //public class ProductService(CoreAPIDbContext context) : IProductService
    //{
    //    private readonly CoreAPIDbContext _context = context;
    //    public async Task<IEnumerable<ProductViewModel>> GetAllProductsAsync()
    //    {
    //        return await _context.Products
    //            .Where(p => p.IsActive)
    //            .Include(p => p.Category)
    //            .Include(p => p.ProductImages)
    //            .Select(p => new ProductViewModel
    //            {
    //                Id = p.Id,
    //                Title = p.Title,
    //                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
    //                Price = p.Price,
    //                DiscountPrice = p.DiscountPrice,
    //                IsDiscounted = p.IsDiscounted,
    //                IsNewArrival = p.IsNewArrival,
    //                IsTrending = p.IsTrending,
    //                ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : null
    //            })
    //            .ToListAsync();
    //    }
    //    public async Task<IEnumerable<ProductViewModel>> GetTrendingProductsAsync()
    //    {
    //        return await _context.Products
    //            .Where(p => p.IsTrending && p.IsActive)
    //            .Include(p => p.Category)
    //            .Include(p => p.ProductImages)
    //            .Select(p => new ProductViewModel
    //            {
    //                Id = p.Id,
    //                Title = p.Title,
    //                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
    //                Price = p.Price,
    //                DiscountPrice = p.DiscountPrice,
    //                IsDiscounted = p.IsDiscounted,
    //                IsNewArrival = p.IsNewArrival,
    //                IsTrending = p.IsTrending,
    //                ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().ImageUrl : null
    //            })
    //            .Take(10)
    //            .ToListAsync();
    //    }
    //}
}