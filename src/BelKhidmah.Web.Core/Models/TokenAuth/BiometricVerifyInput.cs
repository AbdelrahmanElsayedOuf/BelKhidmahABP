using System;
using System.ComponentModel.DataAnnotations;

namespace BelKhidmah.Models.TokenAuth
{
    public class BiometricVerifyInput
    {
        [Required]
        public Guid KeyId { get; set; }

        [Required]
        [MaxLength(64)]
        public string Nonce { get; set; }

        [Required]
        [MaxLength(256)]
        public string Signature { get; set; }
    }
}
