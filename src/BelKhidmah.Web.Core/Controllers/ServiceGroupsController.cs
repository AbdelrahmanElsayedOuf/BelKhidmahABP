using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    [Route("api/servicegroups")]
    public class ServiceGroupsController : BelKhidmahProxyControllerBase
    {
        public ServiceGroupsController(IHttpClientFactory factory) : base(factory) { }

        [HttpGet("Get")]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/servicegroups/Get"));

        [HttpGet("GetHierarchy")]
        public Task<IActionResult> GetHierarchy()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/servicegroups/GetHierarchy"));
    }
}
