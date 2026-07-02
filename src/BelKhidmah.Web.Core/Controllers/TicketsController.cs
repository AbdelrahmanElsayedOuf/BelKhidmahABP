using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BelKhidmah.Controllers
{
    [Authorize]
    [Route("api/tickets")]
    public class TicketsController : BelKhidmahProxyControllerBase
    {
        public TicketsController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet]
        public Task<IActionResult> GetByCustomer()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/Tickets"));

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var req = BuildRequest(HttpMethod.Post, "api/Tickets");
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
