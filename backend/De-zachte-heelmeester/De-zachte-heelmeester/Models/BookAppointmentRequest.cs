namespace De_zachte_heelmeester.Models;

/// <summary>
/// The appointment a patient wants to book.
/// </summary>
public class BookAppointmentRequest
{
    public int SpecialistId { get; set; }

    public DateOnly Date { get; set; }

    public string StartTime { get; set; } = string.Empty;

    public int DurationQuarters { get; set; }
}
