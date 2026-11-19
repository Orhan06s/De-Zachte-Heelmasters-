using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Specialization { get; set; } = string.Empty;

        public User User { get; set; } = null!;

        public ICollection<TimeSlot> TimeSlots { get; set; }
            = new List<TimeSlot>();
    }
}
