using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [Route("api/payment")]
    public class PaymentController : BelKhidmahProxyControllerBase
    {
        public PaymentController(IHttpClientFactory factory, IConfiguration configuration) : base(factory, configuration) { }

        [HttpGet("InitializePay/{id}")]
        public Task<IActionResult> InitializePay(string id)
            => ProxyAsync(BuildRequest(HttpMethod.Get, $"api/PurchasedOrder/InitializePay/{id}"));

        [HttpGet("OnlinePay")]
        public Task<IActionResult> OnlinePay()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/PurchasedOrder/OnlinePay"));
    }
}
