using Abp.MultiTenancy;
using BelKhidmah.Authorization.Users;

namespace BelKhidmah.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
