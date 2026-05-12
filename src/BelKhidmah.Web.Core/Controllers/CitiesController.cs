using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    [Route("api/cities")]
    public class CitiesController : BelKhidmahProxyControllerBase
    {
        public CitiesController(IHttpClientFactory factory) : base(factory) { }

        [HttpGet]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/cities"));
    }
}
