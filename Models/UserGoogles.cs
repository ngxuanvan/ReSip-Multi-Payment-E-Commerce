namespace ResipWeb.Models
{
    public class UserGoogles
    {
        public int Id { get; set; }
        public string GoogleId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
