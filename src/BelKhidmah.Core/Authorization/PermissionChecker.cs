using Abp.Authorization;
using BelKhidmah.Authorization.Roles;
using BelKhidmah.Authorization.Users;

namespace BelKhidmah.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
