using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace BelKhidmah.Controllers
{
    [Authorize]
    [Route("api/faqs")]
    public class FAQsController : BelKhidmahProxyControllerBase
    {
        public FAQsController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet]
        public Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            => ProxyAsync(BuildRequest(HttpMethod.Get, $"api/FAQs"));
    }
}
