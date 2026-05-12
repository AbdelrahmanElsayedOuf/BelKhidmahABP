using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/servicesubtype")]
    public class ServiceSubTypeController : BelKhidmahProxyControllerBase
    {
        public ServiceSubTypeController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet("GetByServiceGroup")]
        public Task<IActionResult> GetByServiceGroup()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/servicesubtype/GetByServiceGroup"));
    }
}
