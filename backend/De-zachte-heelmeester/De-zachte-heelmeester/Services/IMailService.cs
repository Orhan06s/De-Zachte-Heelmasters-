namespace De_zachte_heelmeester.Services
{
    /// <summary>
    /// Sends emails. Can later be replaced by a real mail service.
    /// </summary>
    public interface IMailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}