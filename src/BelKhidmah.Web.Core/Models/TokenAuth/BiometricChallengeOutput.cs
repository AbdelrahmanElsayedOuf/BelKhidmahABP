using System;

namespace BelKhidmah.Models.TokenAuth
{
    public class BiometricChallengeOutput
    {
        public Guid KeyId { get; set; }

        public string Nonce { get; set; }

        public int ExpiresInSeconds { get; set; }
    }
}
