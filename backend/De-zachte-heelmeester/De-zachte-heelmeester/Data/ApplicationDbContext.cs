
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
        public DbSet<LoginLink> LoginLinks => Set<LoginLink>();

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

            // AppointmentStatus stored as string
            builder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>();

            // BSN configuration
            builder.Entity<Patient>()
                .Property(p => p.BSN)
                .HasMaxLength(9)
                .IsRequired();

            // Refresh token hash configuration
            builder.Entity<UserSession>()
                .Property(s => s.RefreshTokenHash)
                .HasMaxLength(256)
                .IsRequired();

            // ApplicationUser - LoginLink (1 to many)
            builder.Entity<LoginLink>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LoginLink>()
                .HasIndex(l => l.TokenHash)
                .IsUnique();
        }
    }
}
