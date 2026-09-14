using System;

namespace BelKhidmah.Models.TokenAuth
{
    public class BiometricDeviceDto
    {
        public Guid KeyId { get; set; }

        public string DeviceId { get; set; }

        public string DeviceName { get; set; }

        public string KeyAlgorithm { get; set; }

        public DateTime EnrolledAt { get; set; }

        public DateTime? LastUsedAt { get; set; }
    }
}
