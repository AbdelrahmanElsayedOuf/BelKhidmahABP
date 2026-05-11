using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    [Route("api/providerpackages")]
    public class ProviderPackagesController : BelKhidmahProxyControllerBase
    {
        public ProviderPackagesController(IHttpClientFactory factory) : base(factory) { }

        [HttpGet("Get")]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/providerpackages/Get"));

        [HttpGet("{packageId}/detail")]
        public Task<IActionResult> GetDetail([FromRoute] Guid packageId)
            => ProxyAsync(BuildRequest(HttpMethod.Get, $"api/providerpackages/{packageId}/detail"));
    }
}
