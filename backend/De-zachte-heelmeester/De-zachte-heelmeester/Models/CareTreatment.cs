using System.ComponentModel.DataAnnotations;

namespace De_zachte_heelmeester.Models;

/// <summary>
/// One row from the list of care treatments of the hospital, for example Z0001 "Consult hart".
/// </summary>
public class CareTreatment
{
    [Key]
    [StringLength(10)]
    public string CareCode { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Specialization { get; set; } = string.Empty;

    /// <summary>
    /// How long the treatment takes, in quarters of an hour.
    /// </summary>
    public int DurationQuarters { get; set; }
}
