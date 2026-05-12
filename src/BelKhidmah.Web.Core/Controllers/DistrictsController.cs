using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/districts")]
    public class DistrictsController : BelKhidmahProxyControllerBase
    {
        public DistrictsController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet("GetByCity")]
        public Task<IActionResult> GetByCity()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/districts/GetByCity"));
    }
}
