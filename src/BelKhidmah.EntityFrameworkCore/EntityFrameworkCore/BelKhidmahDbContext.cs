using System;
using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using BelKhidmah.Authorization.Roles;
using BelKhidmah.Authorization.Users;
using BelKhidmah.MultiTenancy;
using BelKhidmah.Otp;

namespace BelKhidmah.EntityFrameworkCore
{
    public class BelKhidmahDbContext : AbpZeroDbContext<Tenant, Role, User, BelKhidmahDbContext>
    {
        public DbSet<OtpCode> OtpCodes { get; set; }

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
        }
    }
}
