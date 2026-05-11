using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    [Route("api/bookings")]
    public class BookingsController : BelKhidmahProxyControllerBase
    {
        public BookingsController(IHttpClientFactory factory) : base(factory) { }

        [HttpPost("Create")]
        public async Task<IActionResult> Create()
        {
            var req = BuildRequest(HttpMethod.Post, "api/bookings/Create");
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
