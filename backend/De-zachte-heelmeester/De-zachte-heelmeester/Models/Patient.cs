using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models
{

    public class Patient
    {
        [Key]
        [StringLength(9)]
        public string BSN { get; set; } = string.Empty;

        public string? UserId { get; set; } = string.Empty;

        public string? FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; } = string.Empty;

        public User User { get; set; } = null!;

        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}

