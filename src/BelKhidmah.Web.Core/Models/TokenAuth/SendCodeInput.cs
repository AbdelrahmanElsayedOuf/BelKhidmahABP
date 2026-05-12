using System.ComponentModel.DataAnnotations;

namespace BelKhidmah.Models.TokenAuth
{
    public class SendCodeInput
    {
        [Required]
        [Phone]
        [MaxLength(32)]
        public string PhoneNumber { get; set; }
    }
}
