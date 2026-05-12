using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/servicegroups")]
    public class ServiceGroupsController : BelKhidmahProxyControllerBase
    {
        public ServiceGroupsController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet("Get")]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/servicegroups/Get"));

        [HttpGet("GetHierarchy")]
        public Task<IActionResult> GetHierarchy()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/servicegroups/GetHierarchy"));
    }
}
