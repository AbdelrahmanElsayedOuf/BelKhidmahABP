using Abp.Application.Services.Dto;

namespace BelKhidmah.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

