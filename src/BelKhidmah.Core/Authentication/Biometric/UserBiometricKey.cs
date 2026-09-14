using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace BelKhidmah.Authentication.Biometric
{
    public class UserBiometricKey : Entity<Guid>, IHasCreationTime, ISoftDelete
    {
        public const int MaxDeviceIdLength = 128;
        public const int MaxDeviceNameLength = 128;
        public const int MaxPublicKeyLength = 2048;
        public const int MaxKeyAlgorithmLength = 16;

        public const string AlgorithmEs256 = "ES256";

        public long UserId { get; set; }

        public string DeviceId { get; set; }

        public string DeviceName { get; set; }

        public string PublicKey { get; set; }

        public string KeyAlgorithm { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastUsedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
