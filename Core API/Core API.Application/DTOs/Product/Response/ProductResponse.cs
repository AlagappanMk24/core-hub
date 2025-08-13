namespace Core_API.Application.DTOs.Product.Response
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public double Price { get; set; }
        public double DiscountPrice { get; set; }
        public bool IsDiscounted { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsTrending { get; set; }
        public string ImageUrl { get; set; }
    }
}
