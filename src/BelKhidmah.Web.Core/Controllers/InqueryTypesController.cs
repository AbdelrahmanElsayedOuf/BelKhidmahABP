using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/InqueryTypes")]
    public class InqueryTypesController : BelKhidmahProxyControllerBase
    {
        public InqueryTypesController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/InqueryTypes"));
    }
}
