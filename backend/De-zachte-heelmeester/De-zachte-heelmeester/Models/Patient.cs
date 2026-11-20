using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Models
{
    
    [Index(nameof(BSN), IsUnique = true)]
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        
        [StringLength(9)]
        public string BSN { get; set; } = string.Empty;
        
        public string? UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public User? User { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}

