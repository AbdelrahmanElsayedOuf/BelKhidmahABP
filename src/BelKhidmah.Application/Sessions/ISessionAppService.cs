using System.Threading.Tasks;
using Abp.Application.Services;
using BelKhidmah.Sessions.Dto;

namespace BelKhidmah.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
