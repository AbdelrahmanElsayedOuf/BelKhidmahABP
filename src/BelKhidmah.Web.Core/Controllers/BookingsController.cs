using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace BelKhidmah.Controllers
{
    [Authorize]
    [Route("api/bookings")]
    public class BookingsController : BelKhidmahProxyControllerBase
    {
        public BookingsController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpPost("Create")]
        public async Task<IActionResult> Create()
        {
            var req = BuildRequest(HttpMethod.Post, "api/bookings/Create");

            req.Content = await ReadBodyAsJsonContent();
            return await ProxyAsync(req);
        }

        [HttpGet("items")]
        public async Task<IActionResult> Items(int sector , int page , int pageSize)
        {
            var req = BuildRequest(HttpMethod.Get, "api/Bookings/Items");
            req.Content = await ReadBodyAsJsonContent();
            return await ProxyAsync(req);
        }
        [HttpGet("Items/{bookingItemId}/Details")]
        public async Task<IActionResult> Details(Guid bookingItemId)
        {
            var req = BuildRequest(
                HttpMethod.Get,
                $"api/Bookings/Items/{bookingItemId}/Details"); 
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
