using Microsoft.AspNetCore.Mvc;

//namespace Core_API.Web.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class HomeController : ControllerBase
//    {
//        private readonly IProductService _productService;
//        public HomeController(IProductService productService)
//        {
//            _productService = productService;
//        }

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
