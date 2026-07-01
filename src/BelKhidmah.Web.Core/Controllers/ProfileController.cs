using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Authorize]
    [Route("api/profile")]
    public class ProfileController : BelKhidmahProxyControllerBase
    {
        public ProfileController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/Profile"));

        [HttpPut]
        public async Task<IActionResult> Update()
        {
            var req = BuildRequest(HttpMethod.Put, "api/Profile");
            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");
            return await ProxyAsync(req);
        }

        [HttpDelete]
        public Task<IActionResult> Delete()
            => ProxyAsync(BuildRequest(HttpMethod.Delete, "api/Profile"));
    }
}
