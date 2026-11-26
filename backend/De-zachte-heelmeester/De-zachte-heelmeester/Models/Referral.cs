using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models;

public class Referral
{
    [Key]
    public int ReferralId { get; set; }

    [Required]
    public string PatientBSN { get; set; } = string.Empty;

    [Required]
    public string CareCode { get; set; } = string.Empty;

    [Required]
    public string Specialization { get; set; } = string.Empty;

    public int DurationMinutes { get; set; } = 30;
}