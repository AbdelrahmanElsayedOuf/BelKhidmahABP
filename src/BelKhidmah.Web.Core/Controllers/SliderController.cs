using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/slider")]
    public class SliderController : BelKhidmahProxyControllerBase
    {
        public SliderController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet("GetAll")]
        public Task<IActionResult> GetAll()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/Slider/GetAll"));

        [HttpGet("GetByCode")]
        public Task<IActionResult> GetByCode([FromQuery] string code)
            => ProxyAsync(BuildRequest(HttpMethod.Get, $"api/Slider/GetByCode"));

        [HttpGet("GetItemsBySliderCode")]
        public Task<IActionResult> GetItemsBySliderCode([FromQuery] string code)
            => ProxyAsync(BuildRequest(HttpMethod.Get, $"api/Slider/GetItemsBySliderCode"));
    }
}
