using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using BelKhidmah.EntityFrameworkCore;
using BelKhidmah.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace BelKhidmah.Web.Tests
{
    [DependsOn(
        typeof(BelKhidmahWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class BelKhidmahWebTestModule : AbpModule
    {
        public BelKhidmahWebTestModule(BelKhidmahEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(BelKhidmahWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(BelKhidmahWebMvcModule).Assembly);
        }
    }
}