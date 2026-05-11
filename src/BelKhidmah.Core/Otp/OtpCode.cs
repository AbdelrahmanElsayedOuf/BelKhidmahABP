using System;
using Abp.Domain.Entities;

namespace BelKhidmah.Otp
{
    public class OtpCode : Entity<Guid>
    {
        public const int CodeLength = 6;
        public const int MaxRecipientLength = 256;

        public string EmailOrPhone { get; set; }
        public string Code { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
