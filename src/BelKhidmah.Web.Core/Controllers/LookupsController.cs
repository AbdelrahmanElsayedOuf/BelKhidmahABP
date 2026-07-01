using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/lookups")]
    public class LookupsController : BelKhidmahProxyControllerBase
    {
        public LookupsController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet("cities")]
        public Task<IActionResult> GetCities()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/Lookups/Cities"));

        [HttpGet("professions")]
        public Task<IActionResult> GetProfessions()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/Lookups/Professions"));

        [HttpGet("nationalities")]
        public Task<IActionResult> GetNationalities()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/Lookups/Nationalities"));
    }
}
