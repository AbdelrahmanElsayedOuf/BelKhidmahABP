using System.ComponentModel.DataAnnotations;

namespace BelKhidmah.Models.TokenAuth
{
    public class VerifyCodeInput
    {
        [Required]
        [MaxLength(256)]
        public string EmailOrPhone { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; }
    }
}
