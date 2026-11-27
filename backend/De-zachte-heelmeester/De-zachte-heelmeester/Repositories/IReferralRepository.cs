using De_zachte_heelmeester.Models;

namespace De_zachte_heelmeester.Repositories;

public interface IReferralRepository
{
    Task<Referral?> GetByIdAsync(int referralId);
    Task<List<Referral>> GetByPatientBsnAsync(string bsn);
}