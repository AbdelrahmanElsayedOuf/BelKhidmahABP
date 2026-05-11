using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using BelKhidmah.Authorization;

namespace BelKhidmah
{
    [DependsOn(
        typeof(BelKhidmahCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class BelKhidmahApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<BelKhidmahAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(BelKhidmahApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
