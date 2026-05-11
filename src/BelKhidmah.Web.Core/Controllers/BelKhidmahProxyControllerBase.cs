using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BelKhidmah.Controllers
{
    public abstract class BelKhidmahProxyControllerBase : BelKhidmahControllerBase
    {
        private static readonly string[] ForwardedHeaders = { "language", "customerId", "tenant" };

        protected readonly HttpClient Client;

        protected BelKhidmahProxyControllerBase(IHttpClientFactory factory)
        {
            Client = factory.CreateClient("ExternalApi");
        }

        protected HttpRequestMessage BuildRequest(HttpMethod method, string relativePath)
        {
            var qs = Request.QueryString.Value;
            var uri = string.IsNullOrEmpty(qs) ? relativePath : relativePath + qs;

            var req = new HttpRequestMessage(method, uri);

            foreach (var header in ForwardedHeaders)
            {
                if (Request.Headers.TryGetValue(header, out var value))
                    req.Headers.TryAddWithoutValidation(header, (string)value);
            }

            return req;
        }

        protected async Task<IActionResult> ProxyAsync(HttpRequestMessage req)
        {
            var response = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode,
                System.Text.Json.JsonSerializer.Deserialize<object>(
                    content.Length > 0 ? content : "null"));
        }
    }
}
