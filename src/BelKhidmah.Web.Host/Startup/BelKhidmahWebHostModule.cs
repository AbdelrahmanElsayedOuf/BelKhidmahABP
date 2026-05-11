using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using BelKhidmah.Configuration;

namespace BelKhidmah.Web.Host.Startup
{
    [DependsOn(
       typeof(BelKhidmahWebCoreModule))]
    public class BelKhidmahWebHostModule: AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public BelKhidmahWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(BelKhidmahWebHostModule).GetAssembly());
        }
    }
}
