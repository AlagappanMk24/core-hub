//using Core_API.Application.Contracts.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace Core_API.Web.Controllers
//{
//    [Route("api/home")]
//    [ApiController]
//    public class HomeController(IProductService productService) : ControllerBase
//    {
//        private readonly IProductService _productService = productService;

//        [HttpGet]
//        public async Task<IActionResult> GetAllProducts()
//        {
//            var products = await _productService.GetAllProductsAsync();
//            return Ok(products);
//        }

//        [HttpGet("trending")]
//        public async Task<IActionResult> GetTrendingProducts()
//        {
//            var products = await _productService.GetTrendingProductsAsync();
//            return Ok(products);
//        }
//    }
//}
