using System.ComponentModel.DataAnnotations;

namespace BelKhidmah.Models.TokenAuth
{
    public class VerifyCodeInput
    {
        [Required]
        [MaxLength(256)]
        public string EmailOrPhone { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 4)]
        public string Code { get; set; }
    }
}
