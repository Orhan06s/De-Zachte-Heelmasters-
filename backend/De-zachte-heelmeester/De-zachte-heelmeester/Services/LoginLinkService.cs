using System.Security.Cryptography;
using System.Text;
using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Services
{
    public enum LoginLinkStatus
    {
        Success,
        Expired,
        Invalid
    }

    public record LoginLinkResult(LoginLinkStatus Status, string? UserId);

    /// <summary>
    /// Creates, emails and checks login links that are valid for 5 minutes (LOG-04).
    /// </summary>
    public class LoginLinkService
    {
        private static readonly TimeSpan ValidFor = TimeSpan.FromMinutes(5);

        private readonly ApplicationDbContext _context;
        private readonly TimeProvider _clock;
        private readonly IMailService _mail;

        public LoginLinkService(ApplicationDbContext context, TimeProvider clock, IMailService mail)
        {
            _context = context;
            _clock = clock;
            _mail = mail;
        }

        /// <summary>
        /// Creates a new link for the user and returns the token that goes in the email.
        /// </summary>
        public async Task<string> CreateLinkAsync(string userId)
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var now = _clock.GetUtcNow().UtcDateTime;

            _context.LoginLinks.Add(new LoginLink
            {
                UserId = userId,
                TokenHash = Hash(token),
                ExpiresAt = now.Add(ValidFor)
            });
            await _context.SaveChangesAsync();

            return token;
        }

        /// <summary>
        /// Creates a link and emails it to the user.
        /// </summary>
        public async Task SendLinkAsync(string userId, string email, string verifyUrl)
        {
            var token = await CreateLinkAsync(userId);
            var link = $"{verifyUrl}?token={token}";

            var body =
                "Klik op de link hieronder om in te loggen. De link is 5 minuten geldig.\n\n" +
                $"{link}\n\n" +
                "Heb je niet geprobeerd in te loggen? Dan kun je deze e-mail negeren.";

            await _mail.SendAsync(email, "Inloggen bij De zachte heelmeesters", body);
        }

        /// <summary>
        /// Checks the token from the link. A link works once and only within 5 minutes.
        /// </summary>
        public async Task<LoginLinkResult> VerifyLinkAsync(string token)
        {
            var hash = Hash(token);
            var link = await _context.LoginLinks.FirstOrDefaultAsync(l => l.TokenHash == hash);

            if (link == null || link.UsedAt != null)
            {
                return new LoginLinkResult(LoginLinkStatus.Invalid, null);
            }

            var now = _clock.GetUtcNow().UtcDateTime;
            if (now > link.ExpiresAt)
            {
                return new LoginLinkResult(LoginLinkStatus.Expired, null);
            }

            link.UsedAt = now;
            await _context.SaveChangesAsync();

            return new LoginLinkResult(LoginLinkStatus.Success, link.UserId);
        }

        private static string Hash(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }
    }
}