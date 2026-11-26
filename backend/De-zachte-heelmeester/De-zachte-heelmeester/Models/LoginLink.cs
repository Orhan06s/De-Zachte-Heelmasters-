using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models
{
    public class LoginLink
    {
        [Key]
        public int LoginLinkId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Only the hash is stored, never the token itself
        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public User User { get; set; } = null!;
    }
}