using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/providers")]
    public class ProvidersController : BelKhidmahProxyControllerBase
    {
        public ProvidersController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet("GetAll")]
        public Task<IActionResult> GetAll()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/providers/GetAll"));
    }
}
