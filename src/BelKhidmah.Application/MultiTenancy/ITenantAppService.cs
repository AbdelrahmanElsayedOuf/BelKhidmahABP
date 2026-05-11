using Abp.Application.Services;
using BelKhidmah.MultiTenancy.Dto;

namespace BelKhidmah.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

