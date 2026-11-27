namespace DeZachteHeelmeestersTests.Appointments;

/// <summary>
/// A logged-in test patient: the client with the login cookie and the BSN of the patient.
/// </summary>
public sealed record TestPatient(HttpClient Client, string Bsn) : IDisposable
{
    public void Dispose() => Client.Dispose();
}
