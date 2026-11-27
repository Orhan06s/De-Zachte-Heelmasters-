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

        // The days this specialist works. The screen to change them comes with AFS-08.
        public WorkDays WorkDays { get; set; } = WorkDays.MondayToFriday;

        public User User { get; set; } = null!;

        public ICollection<TimeSlot> TimeSlots { get; set; }
            = new List<TimeSlot>();
    }
}
