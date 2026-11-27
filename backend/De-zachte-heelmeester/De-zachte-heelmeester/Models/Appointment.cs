using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public string PatientBSN { get; set; } = String.Empty;

        [Required]
        public int TimeSlotId { get; set; }

        [Required]
        public int ReferralId { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; }
            = AppointmentStatus.Scheduled;

        public Patient Patient { get; set; } = null!;

        public TimeSlot TimeSlot { get; set; } = null!;

        public Referral Referral { get; set; } = null!;
    }
}
