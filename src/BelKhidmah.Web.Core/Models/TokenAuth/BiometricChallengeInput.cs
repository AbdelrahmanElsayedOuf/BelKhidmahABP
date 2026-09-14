using System;
using System.ComponentModel.DataAnnotations;

namespace BelKhidmah.Models.TokenAuth
{
    public class BiometricChallengeInput
    {
        [Required]
        public Guid KeyId { get; set; }
    }
}
