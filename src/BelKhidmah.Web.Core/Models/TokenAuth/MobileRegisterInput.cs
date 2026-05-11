using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;

namespace BelKhidmah.Models.TokenAuth
{
    public class MobileRegisterInput
    {
        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        [Phone]
        [StringLength(32)]
        public string PhoneNumber { get; set; }
    }
}
