using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models
{
    
    public class Patient
    {
        [Key]
        [StringLength(9)]
        public string BSN { get; set; } = string.Empty;
        
        public string? UserId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; } 

        [EmailAddress]
        public string? Email { get; set; }

        public User? User { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}

