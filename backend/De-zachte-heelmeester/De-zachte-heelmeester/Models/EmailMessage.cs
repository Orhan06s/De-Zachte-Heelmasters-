using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models
{
    public class EmailMessage
    {
        [Key]
        public int EmailMessageId { get; set; }

        [Required]
        public string To { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? SentAt { get; set; }
    }
}