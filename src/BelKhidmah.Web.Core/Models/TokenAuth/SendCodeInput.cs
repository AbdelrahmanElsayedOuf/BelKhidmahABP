using System.ComponentModel.DataAnnotations;

namespace BelKhidmah.Models.TokenAuth
{
    public class SendCodeInput
    {
        [Required]
        [MaxLength(256)]
        public string EmailOrPhone { get; set; }
    }
}
