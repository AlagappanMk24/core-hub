namespace Core_API.Domain.Entities
{
    public class AuthState
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? UserId { get; set; }
        public string? EmailOTP { get; set; }
        public string? SmsOTP { get; set; }
        public bool PasswordVerified { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
    }
}
