using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using BelKhidmah.Configuration.Dto;

namespace BelKhidmah.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : BelKhidmahAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
