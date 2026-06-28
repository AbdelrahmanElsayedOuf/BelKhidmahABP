using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Authorize]
    [Route("api/favorites")]
    public class FavoritePackagesController : BelKhidmahProxyControllerBase
    {
        public FavoritePackagesController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpPost("{packageId}")]
        public Task<IActionResult> Add([FromRoute] Guid packageId)
            => ProxyAsync(BuildRequest(HttpMethod.Post, $"api/FavoritePackages/{packageId}"));

        [HttpDelete("{packageId}")]
        public Task<IActionResult> Remove([FromRoute] Guid packageId)
            => ProxyAsync(BuildRequest(HttpMethod.Delete, $"api/FavoritePackages/{packageId}"));

        [HttpGet]
        public Task<IActionResult> GetPaged()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/FavoritePackages/Get"));
    }
}
