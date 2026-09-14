using System;
using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using BelKhidmah.Authentication.Biometric;
using BelKhidmah.Authorization.Roles;
using BelKhidmah.Authorization.Users;
using BelKhidmah.MultiTenancy;
using BelKhidmah.Otp;

namespace BelKhidmah.EntityFrameworkCore
{
    public class BelKhidmahDbContext : AbpZeroDbContext<Tenant, Role, User, BelKhidmahDbContext>
    {
        public DbSet<OtpCode> OtpCodes { get; set; }

        public DbSet<UserBiometricKey> UserBiometricKeys { get; set; }

        public DbSet<BiometricChallenge> BiometricChallenges { get; set; }

        public BelKhidmahDbContext(DbContextOptions<BelKhidmahDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OtpCode>(b =>
            {
                b.ToTable("OtpCodes");
                b.Property(e => e.EmailOrPhone).HasMaxLength(OtpCode.MaxRecipientLength).IsRequired();
                b.Property(e => e.Code).HasMaxLength(OtpCode.CodeLength + 2).IsRequired();
                b.HasIndex(e => new { e.EmailOrPhone, e.IsUsed });
            });

            modelBuilder.Entity<UserBiometricKey>(b =>
            {
                b.ToTable("UserBiometricKeys");
                b.Property(e => e.DeviceId).HasMaxLength(UserBiometricKey.MaxDeviceIdLength).IsRequired();
                b.Property(e => e.DeviceName).HasMaxLength(UserBiometricKey.MaxDeviceNameLength);
                b.Property(e => e.PublicKey).HasMaxLength(UserBiometricKey.MaxPublicKeyLength).IsRequired();
                b.Property(e => e.KeyAlgorithm).HasMaxLength(UserBiometricKey.MaxKeyAlgorithmLength).IsRequired();
                b.HasIndex(e => new { e.UserId, e.DeviceId })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<BiometricChallenge>(b =>
            {
                b.ToTable("BiometricChallenges");
                b.Property(e => e.Nonce).HasMaxLength(BiometricChallenge.MaxNonceLength).IsRequired();
                b.HasIndex(e => new { e.KeyId, e.IsUsed, e.ExpiresAt });
            });
        }
    }
}
