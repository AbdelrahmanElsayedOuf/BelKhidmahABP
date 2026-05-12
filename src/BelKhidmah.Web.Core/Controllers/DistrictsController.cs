using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    [Route("api/districts")]
    public class DistrictsController : BelKhidmahProxyControllerBase
    {
        public DistrictsController(IHttpClientFactory factory) : base(factory) { }

        [HttpGet("GetByCity")]
        public Task<IActionResult> GetByCity()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/districts/GetByCity"));
    }
}
