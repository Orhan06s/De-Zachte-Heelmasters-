using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models;

/// <summary>
/// A referral from a general practitioner. The care code says which treatment the patient needs,
/// the specialism and the duration come from the list of care treatments.
/// </summary>
public class Referral
{
    [Key]
    public int ReferralId { get; set; }

    [Required]
    [StringLength(9)]
    public string PatientBSN { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string CareCode { get; set; } = string.Empty;

    public Patient Patient { get; set; } = null!;

    public CareTreatment CareTreatment { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; }
        = new List<Appointment>();
}
