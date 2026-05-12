using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/customerlocations")]
    public class CustomerLocationsController : BelKhidmahProxyControllerBase
    {
        public CustomerLocationsController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet("Get")]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/customerlocations/Get"));

        [HttpPost("Create")]
        public async Task<IActionResult> Create()
        {
            var req = BuildRequest(HttpMethod.Post, "api/customerlocations/Create");
            req.Content = await ReadBodyAsJsonContent();
            return await ProxyAsync(req);
        }

        private async Task<StringContent> ReadBodyAsJsonContent()
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}
