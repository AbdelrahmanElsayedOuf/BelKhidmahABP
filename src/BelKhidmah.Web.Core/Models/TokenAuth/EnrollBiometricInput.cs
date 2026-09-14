using System.ComponentModel.DataAnnotations;

namespace BelKhidmah.Models.TokenAuth
{
    public class EnrollBiometricInput
    {
        [Required]
        [MaxLength(128)]
        public string DeviceId { get; set; }

        [MaxLength(128)]
        public string DeviceName { get; set; }

        [Required]
        [MaxLength(2048)]
        public string PublicKey { get; set; }

        [MaxLength(16)]
        public string KeyAlgorithm { get; set; } = "ES256";
    }
}
