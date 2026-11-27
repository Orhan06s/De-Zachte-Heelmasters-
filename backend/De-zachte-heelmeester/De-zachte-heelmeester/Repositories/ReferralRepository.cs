using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Repositories;

public class ReferralRepository : IReferralRepository
{
    private readonly ApplicationDbContext _context;

    public ReferralRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // The care treatment is loaded as well, because the specialism and the duration come from there.
    public async Task<Referral?> GetByIdAsync(int referralId)
    {
        return await _context.Referrals
            .Include(r => r.CareTreatment)
            .FirstOrDefaultAsync(r => r.ReferralId == referralId);
    }

    public async Task<List<Referral>> GetByPatientBsnAsync(string bsn)
    {
        return await _context.Referrals
            .Include(r => r.CareTreatment)
            .Where(r => r.PatientBSN == bsn)
            .OrderByDescending(r => r.ReferralId)
            .ToListAsync();
    }
}
