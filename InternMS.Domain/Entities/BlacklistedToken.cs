namespace InternMS.Domain.Entities
{
    public class BlacklistedToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public DateTime RevokedAt { get; set; }
    }
}