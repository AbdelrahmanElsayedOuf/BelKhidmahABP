using System.ComponentModel.DataAnnotations;

namespace BelKhidmah.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}