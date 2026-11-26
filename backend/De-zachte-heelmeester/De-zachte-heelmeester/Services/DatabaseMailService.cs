using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Services
{
    /// <summary>
    /// Stores emails in the database, because there is no free mail service yet.
    /// </summary>
    public class DatabaseMailService : IMailService
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeProvider _clock;

        public DatabaseMailService(ApplicationDbContext context, TimeProvider clock)
        {
            _context = context;
            _clock = clock;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            _context.EmailMessages.Add(new EmailMessage
            {
                To = to,
                Subject = subject,
                Body = body,
                CreatedAt = _clock.GetUtcNow().UtcDateTime
            });
            await _context.SaveChangesAsync();
        }
    }
}