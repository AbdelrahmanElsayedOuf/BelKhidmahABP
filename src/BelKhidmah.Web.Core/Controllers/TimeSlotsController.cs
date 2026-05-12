using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    [Route("api/timeslots")]
    public class TimeSlotsController : BelKhidmahProxyControllerBase
    {
        public TimeSlotsController(IHttpClientFactory factory) : base(factory) { }

        [HttpGet]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/timeslots"));
    }
}
