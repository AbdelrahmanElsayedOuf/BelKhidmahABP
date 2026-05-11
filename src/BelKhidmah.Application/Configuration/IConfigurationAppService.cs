using System.Threading.Tasks;
using BelKhidmah.Configuration.Dto;

namespace BelKhidmah.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
