namespace Core_API.Application.DTOs.Customer.Response
{
    public class CustomerStatsDto
    {
        public int AllCount { get; set; }
        public double AllChange { get; set; }
        public int ActiveCount { get; set; }
        public double ActiveChange { get; set; }
        public int InactiveCount { get; set; }
        public double InactiveChange { get; set; }
    }
}
