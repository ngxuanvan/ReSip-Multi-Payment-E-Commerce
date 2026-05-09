using System.ComponentModel.DataAnnotations;

namespace ResipWeb.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = "";

        [Required, StringLength(50)]
        public string Username { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? ShippingAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public string? Role { get; set; }

    }
}
