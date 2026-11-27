
using De_zachte_heelmeester.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace De_zachte_heelmeester.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<Referral> Referrals => Set<Referral>();
        public DbSet<CareTreatment> CareTreatments => Set<CareTreatment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Patient - ApplicationUser (1 to 0..1)
            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Patient>(p => p.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Patient>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            // Employee - ApplicationUser (1 to 1)
            builder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Employee>(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .HasIndex(e => e.UserId)
                .IsUnique();

            // ApplicationUser - UserSession (1 to many)
            builder.Entity<UserSession>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Employee - TimeSlot (1 to many)
            builder.Entity<TimeSlot>()
                .HasOne(t => t.Employee)
                .WithMany(e => e.TimeSlots)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient - Appointment (1 to many)
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientBSN)
                .OnDelete(DeleteBehavior.Restrict);

            // TimeSlot - Appointment (1 to many)
            builder.Entity<Appointment>()
                .HasOne(a => a.TimeSlot)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TimeSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Referral - Appointment (1 to many)
            builder.Entity<Appointment>()
                .HasOne(a => a.Referral)
                .WithMany(r => r.Appointments)
                .HasForeignKey(a => a.ReferralId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient - Referral (1 to many)
            builder.Entity<Referral>()
                .HasOne(r => r.Patient)
                .WithMany()
                .HasForeignKey(r => r.PatientBSN)
                .OnDelete(DeleteBehavior.Restrict);

            // CareTreatment - Referral (1 to many)
            builder.Entity<Referral>()
                .HasOne(r => r.CareTreatment)
                .WithMany()
                .HasForeignKey(r => r.CareCode)
                .OnDelete(DeleteBehavior.Restrict);

            // List of care treatments from the case, filled by the migration
            builder.Entity<CareTreatment>()
                .HasData(CareTreatmentData.All);

            // AppointmentStatus stored as string
            builder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>();

            // BSN configuration
            builder.Entity<Patient>()
                .Property(p => p.BSN)
                .HasMaxLength(9)
                .IsRequired();
            
            // Patients: controles op databaseniveau
            builder.Entity<Patient>()
                .ToTable(p =>
                {
                    // BSN: 9 cijfers en de elfproef (dezelfde regel als BsnValidator in de backend)
                    p.HasCheckConstraint("CK_Patients_BSN", """
                                                            [BSN] <> '000000000'
                                                            AND CASE
                                                                WHEN LEN([BSN]) = 9 AND [BSN] NOT LIKE '%[^0-9]%' THEN
                                                                    ( CAST(SUBSTRING([BSN], 1, 1) AS INT) * 9
                                                                    + CAST(SUBSTRING([BSN], 2, 1) AS INT) * 8
                                                                    + CAST(SUBSTRING([BSN], 3, 1) AS INT) * 7
                                                                    + CAST(SUBSTRING([BSN], 4, 1) AS INT) * 6
                                                                    + CAST(SUBSTRING([BSN], 5, 1) AS INT) * 5
                                                                    + CAST(SUBSTRING([BSN], 6, 1) AS INT) * 4
                                                                    + CAST(SUBSTRING([BSN], 7, 1) AS INT) * 3
                                                                    + CAST(SUBSTRING([BSN], 8, 1) AS INT) * 2
                                                                    - CAST(SUBSTRING([BSN], 9, 1) AS INT) ) % 11
                                                                ELSE 1
                                                            END = 0
                                                            """);

                    // E-mailadres: mag leeg zijn (de huisarts vult het niet in), anders de vorm naam@domein.nl
                    p.HasCheckConstraint("CK_Patients_Email", "[Email] IS NULL OR [Email] LIKE '%_@_%._%'");
                });

            // AspNetUsers: één account per e-mailadres, ook als twee registraties tegelijk binnenkomen
            builder.Entity<User>()
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique()
                .HasFilter("[NormalizedEmail] IS NOT NULL");

            // Refresh token hash configuration
            builder.Entity<UserSession>()
                .Property(s => s.RefreshTokenHash)
                .HasMaxLength(256)
                .IsRequired();
        }
    }
}
