namespace Core_API.Application.DTOs.Cart.Response
{
    public class CartResponse
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public double TotalPrice { get; set; }
    }
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public double? DiscountPrice { get; set; }
        public int Count { get; set; }
        public string ImageUrl { get; set; }
    }
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Count { get; set; } = 1;
    }
}
