using System.Threading.Tasks;
using Abp.Application.Services;
using BelKhidmah.Authorization.Accounts.Dto;

namespace BelKhidmah.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
