using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models
{
    public class UserSession
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RefreshTokenHash { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime LastActivityAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? DeviceInfo { get; set; }

        public string? IpAddress { get; set; }

        public User User { get; set; } = null!;
    }
}
