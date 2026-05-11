using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    [Route("api/providers")]
    public class ProvidersController : BelKhidmahProxyControllerBase
    {
        public ProvidersController(IHttpClientFactory factory) : base(factory) { }

        [HttpGet("GetAll")]
        public Task<IActionResult> GetAll()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/providers/GetAll"));
    }
}
