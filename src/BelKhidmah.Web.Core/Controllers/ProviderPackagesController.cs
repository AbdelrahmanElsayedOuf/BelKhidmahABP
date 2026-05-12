using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/providerpackages")]
    public class ProviderPackagesController : BelKhidmahProxyControllerBase
    {
        public ProviderPackagesController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet("Get")]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/providerpackages/Get"));

        [HttpGet("{packageId}/detail")]
        public Task<IActionResult> GetDetail([FromRoute] Guid packageId)
            => ProxyAsync(BuildRequest(HttpMethod.Get, $"api/providerpackages/{packageId}/detail"));
    }
}
