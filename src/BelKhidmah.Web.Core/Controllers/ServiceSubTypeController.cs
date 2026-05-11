using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    [Route("api/servicesubtype")]
    public class ServiceSubTypeController : BelKhidmahProxyControllerBase
    {
        public ServiceSubTypeController(IHttpClientFactory factory) : base(factory) { }

        [HttpGet("GetByServiceGroup")]
        public Task<IActionResult> GetByServiceGroup()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/servicesubtype/GetByServiceGroup"));
    }
}
