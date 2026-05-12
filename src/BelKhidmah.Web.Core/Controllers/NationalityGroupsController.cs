using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    [Route("api/nationalitygroups")]
    public class NationalityGroupsController : BelKhidmahProxyControllerBase
    {
        public NationalityGroupsController(IHttpClientFactory factory) : base(factory) { }

        [HttpGet]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/nationalitygroups"));
    }
}
