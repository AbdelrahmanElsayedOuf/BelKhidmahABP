using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace BelKhidmah.Controllers
{
    public abstract class BelKhidmahControllerBase: AbpController
    {
        protected BelKhidmahControllerBase()
        {
            LocalizationSourceName = BelKhidmahConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
