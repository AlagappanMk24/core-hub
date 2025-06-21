using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Microsoft.AspNetCore.Http;
namespace Core_API.Infrastructure.Services
{
    //public class ProductService(IUnitOfWork unitOfWork) : IProductService
    //{
    //    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    //    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    //    {
    //        return await _unitOfWork.Products.GetAllAsync(includeProperties: "Category,ProductImages");
    //    }
    //    public async Task<PaginatedResult<ProductDto>> GetProductsPaginatedAsync(ProductQueryParameters parameters)
    //    {
    //        try
    //        {
    //            // Base query
    //            var query = _unitOfWork.Products.Query()
    //                .Include(p => p.Category)
    //                .Include(p => p.ProductImages)
    //                .AsQueryable();
    //            // Apply enhanced search filters
    //            if (!string.IsNullOrEmpty(parameters.SearchTerm))
    //            {
    //                string searchTerm = parameters.SearchTerm.ToLower().Trim();
    //                query = query.Where(p =>
    //                    p.Title.ToLower().Contains(searchTerm) ||
    //                    p.Description.ToLower().Contains(searchTerm) ||
    //                    p.ShortDescription.ToLower().Contains(searchTerm) ||
    //                    p.SKU.ToLower().Contains(searchTerm) ||
    //                    p.Barcode.ToLower().Contains(searchTerm) ||
    //                    p.MetaTitle.ToLower().Contains(searchTerm) ||
    //                    p.MetaDescription.ToLower().Contains(searchTerm) ||
    //                    p.Brand.Name.ToLower().Contains(searchTerm) ||
    //                    p.Category.Name.ToLower().Contains(searchTerm) ||
    //                    (p.Tags != null && p.Tags.Any(t => t.TagName.ToLower().Contains(searchTerm)))
    //                );
    //            }

    //            if (parameters.CategoryId.HasValue)
    //            {
    //                query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);
    //            }

    //            // Apply stock status filter 
    //            if (!string.IsNullOrEmpty(parameters.StockStatus))
    //            {
    //                query = parameters.StockStatus switch
    //                {
    //                    "in-stock" => query.Where(p => p.StockQuantity > 10),
    //                    "low-stock" => query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 10),
    //                    "out-of-stock" => query.Where(p => p.StockQuantity <= 0),
    //                    _ => query
    //                };
    //            }

    //            // Apply sorting
    //            if (!string.IsNullOrEmpty(parameters.SortColumn))
    //            {
    //                query = parameters.SortColumn.ToLower() switch
    //                {
    //                    "title" => parameters.SortDirection == "asc" ?
    //                query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
    //                    "sku" => parameters.SortDirection == "asc" ?
    //                        query.OrderBy(p => p.SKU) : query.OrderByDescending(p => p.SKU),
    //                    "price" => parameters.SortDirection == "asc" ?
    //                        query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
    //                    "brand" => parameters.SortDirection == "asc" ?
    //                        query.OrderBy(p => p.Brand.Name) : query.OrderByDescending(p => p.Brand.Name),
    //                    "category" => parameters.SortDirection == "asc" ?
    //                        query.OrderBy(p => p.Category.Name) : query.OrderByDescending(p => p.Category.Name),
    //                    "rating" => parameters.SortDirection == "asc" ?
    //                        query.OrderBy(p => p.AverageRating) : query.OrderByDescending(p => p.AverageRating),
    //                    "soldcount" => parameters.SortDirection == "asc" ?
    //                        query.OrderBy(p => p.SoldCount) : query.OrderByDescending(p => p.SoldCount),
    //                    "views" => parameters.SortDirection == "asc" ?
    //                        query.OrderBy(p => p.Views) : query.OrderByDescending(p => p.Views),
    //                    "newarrival" => query.OrderByDescending(p => p.IsNewArrival),
    //                    "trending" => query.OrderByDescending(p => p.IsTrending),
    //                    "featured" => query.OrderByDescending(p => p.IsFeatured),
    //                    _ => query.OrderBy(p => p.Title)
    //                };
    //            }

    //            // Get total count before pagination
    //            var totalCount = await query.CountAsync();

    //            // Apply pagination
    //            var items = await query
    //                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
    //                .Take(parameters.PageSize)
    //                .Select(p => new ProductDto
    //                {
    //                    Id = p.Id,
    //                    Title = p.Title,
    //                    ShortDescription = p.ShortDescription,
    //                    SKU = p.SKU,
    //                    Price = p.Price,
    //                    StockQuantity = p.StockQuantity,
    //                    CategoryName = p.Category.Name,
    //                    BrandName = p.Brand.Name,
    //                    IsFeatured = p.IsFeatured,
    //                    IsNewArrival = p.IsNewArrival,
    //                    IsTrending = p.IsTrending,
    //                    Views = p.Views,
    //                    SoldCount = p.SoldCount,
    //                    AverageRating = p.AverageRating,
    //                    ProductImages = p.ProductImages.Select(img => new ProductImage
    //                    {
    //                        Id = img.Id,
    //                        ImageUrl = img.ImageUrl,
    //                        ProductId = img.ProductId
    //                    }).ToList()
    //                })
    //                .ToListAsync();

    //            return new PaginatedResult<ProductDto>
    //            {
    //                Items = items,
    //                TotalCount = totalCount,
    //                PageNumber = parameters.PageNumber,
    //                PageSize = parameters.PageSize
    //            };
    //        }
    //        catch (Exception ex)
    //        {
    //            // Log error
    //            throw;
    //        }
    //    }
    //    public async Task<PaginatedResult<ProductDto>> GetProductsPaginatedForVendorAsync(ProductQueryParameters parameters, string vendorId)
    //    {
    //        var paginatedResult = await GetProductsPaginatedAsync(parameters);
    //        paginatedResult.Items = paginatedResult.Items.Where(p => p.VendorId == vendorId).ToList();
    //        paginatedResult.TotalCount = paginatedResult.Items.Count;
    //        return paginatedResult;
    //    }
    //    public async Task<Product> GetProductByIdAsync(int id)
    //    {
    //        return await _unitOfWork.Products.GetAsync(p => p.Id == id, includeProperties: "Category,ProductImages");
    //    }
    //    public async Task<Product> GetProductByIdForVendorAsync(int id, int vendorId)
    //    {
    //        var product = await _unitOfWork.Products.GetAsync(p => p.Id == id && p.VendorId == vendorId, includeProperties: "Category,Brand,ProductImages");
    //        return product;
    //    }
    //    public async Task<string> UpsertProductAsync(ProductVM productVM, List<IFormFile> files, string webRootPath, int? vendorId)
    //    {
    //        productVM.Product.VendorId = vendorId; // Assign the VendorId to the product
    //        if (productVM.Product.Id == 0)
    //        {
    //            await _unitOfWork.Products.AddAsync(productVM.Product);
    //            await _unitOfWork.SaveAsync();
    //        }
    //        else
    //        {
    //            var existingProduct = await _unitOfWork.Products.GetAsync(p => p.Id == productVM.Product.Id && p.VendorId == vendorId);
    //            if (existingProduct == null)
    //                throw new UnauthorizedAccessException("You are not authorized to update this product.");

    //            _unitOfWork.Products.Update(productVM.Product);
    //            await _unitOfWork.SaveAsync();
    //        }

    //        if (files != null && files.Any())
    //        {
    //            string productPath = $@"images\products\product-{productVM.Product.Id}";
    //            foreach (var file in files)
    //            {
    //                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
    //                string finalPath = Path.Combine(webRootPath, productPath);

    //                if (!Directory.Exists(finalPath)) Directory.CreateDirectory(finalPath);

    //                using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
    //                {
    //                    file.CopyTo(fileStream);
    //                }

    //                productVM.Product.ProductImages ??= new List<ProductImage>();
    //                productVM.Product.ProductImages.Add(new ProductImage
    //                {
    //                    ImageUrl = $@"\{productPath}\{fileName}",
    //                    ProductId = productVM.Product.Id
    //                });
    //            }
    //            _unitOfWork.Products.Update(productVM.Product);
    //            await _unitOfWork.SaveAsync();
    //        }
    //        return productVM.Product.Id == 0 ? "Product created successfully" : "Product updated successfully";
    //    }
    //    public async Task<string> DeleteProductImageAsync(int imageId, string webRootPath)
    //    {
    //        var imageToBeDeleted = await _unitOfWork.ProductImages.GetAsync(u => u.Id == imageId);
    //        if (imageToBeDeleted == null) return "Image not found";

    //        var imagePath = Path.Combine(webRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));
    //        if (File.Exists(imagePath)) File.Delete(imagePath);

    //        await _unitOfWork.ProductImages.RemoveAsync(imageToBeDeleted);
    //        await _unitOfWork.SaveAsync();
    //        return "Image deleted successfully";
    //    }
    //    public async Task<object> DeleteProductAsync(int id, string webRootPath)
    //    {
    //        var productToBeDeleted = await _unitOfWork.Products.GetAsync(u => u.Id == id);
    //        if (productToBeDeleted == null) return new { success = false, message = "Error while deleting" };

    //        string productPath = $@"images\products\product-{id}";
    //        string finalPath = Path.Combine(webRootPath, productPath);
    //        if (Directory.Exists(finalPath))
    //        {
    //            foreach (var filePath in Directory.GetFiles(finalPath))
    //            {
    //                File.Delete(filePath);
    //            }
    //        }

    //        await _unitOfWork.Products.RemoveAsync(productToBeDeleted);
    //        await _unitOfWork.SaveAsync();
    //        return new { success = true, message = "Product deleted successfully" };
    //    }

    //    // GET list of Low stock products
    //    public async Task<List<Product>> GetLowStockProductsAsync()
    //    {
    //        int lowStockThreshold = 5;

    //        return (await _unitOfWork.Products.GetAllAsync(
    //            filter: p => p.StockQuantity < lowStockThreshold,
    //            includeProperties: "ProductImages,Category,Vendor"
    //        )).ToList();
    //    }

    //    // GET Maximum sold out products
    //    public async Task<List<TopSoldProductViewModel>> GetTopSoldProductsAsync(int topCount)
    //    {
    //        // Use OrderDetails instead of OrderItems
    //        var allOrderDetails = await _unitOfWork.OrderDetails.GetAllAsync();

    //        var topSoldProducts = allOrderDetails
    //            .GroupBy(od => od.ProductId)
    //            .Select(group => new
    //            {
    //                ProductId = group.Key,
    //                TotalSold = group.Sum(od => od.Count), // Use Count instead of Quantity
    //            })
    //            .OrderByDescending(p => p.TotalSold)
    //            .Take(topCount)
    //            .ToList();

    //        var productIds = topSoldProducts.Select(p => p.ProductId).ToList();

    //        var products = (await _unitOfWork.Products.GetAllAsync(
    //            filter: p => productIds.Contains(p.Id),
    //            includeProperties: "Category,Vendor,ProductImages"
    //        )).ToList();

    //        var result = topSoldProducts
    //            .Join(products,
    //                sold => sold.ProductId,
    //                product => product.Id,
    //                (sold, product) => new TopSoldProductViewModel
    //                {
    //                    ProductId = product.Id,
    //                    ProductName = product.Title,
    //                    ProductPrice = Convert.ToDecimal(product.Price),
    //                    Category = product.Category.Name,
    //                    ProductImage = product.ProductImages.ToList(),
    //                    Vendor = product.Vendor,
    //                    TotalSold = sold.TotalSold,
    //                })
    //            .ToList();

    //        return result;
    //    }
    //}
}
