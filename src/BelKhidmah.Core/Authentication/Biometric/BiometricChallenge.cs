using System;
using Abp.Domain.Entities;

namespace BelKhidmah.Authentication.Biometric
{
    public class BiometricChallenge : Entity<Guid>
    {
        public const int MaxNonceLength = 64;

        public Guid KeyId { get; set; }

        public string Nonce { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
