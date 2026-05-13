using System.Net.Http;
using System.Threading.Tasks;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Controllers
{
    [WrapResult(false, false)]
    public abstract class BelKhidmahProxyControllerBase : BelKhidmahControllerBase
    {
        private static readonly string[] ForwardedHeaders = { "language", "tenant" };

        protected readonly HttpClient Client;
        private readonly string _apiKey;

        protected BelKhidmahProxyControllerBase(IHttpClientFactory factory, IConfiguration configuration)
        {
            Client = factory.CreateClient("ExternalApi");
            _apiKey = configuration["ExternalApi:ApiKey"];
        }

        private string ResolveLanguage()
        {
            var acceptLanguage = Request.Headers["Accept-Language"].ToString();
            return acceptLanguage.StartsWith("ar", System.StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
        }

        protected HttpRequestMessage BuildRequest(HttpMethod method, string relativePath)
        {
            var lang = ResolveLanguage();
            var qs   = Request.QueryString.Value;
            var uri  = $"{lang}/{relativePath}";
            if (!string.IsNullOrEmpty(qs)) uri += qs;

            var req = new HttpRequestMessage(method, uri);

            foreach (var header in ForwardedHeaders)
            {
                if (Request.Headers.TryGetValue(header, out var value))
                    req.Headers.TryAddWithoutValidation(header, (string)value);
            }

            if (!string.IsNullOrEmpty(_apiKey))
                req.Headers.TryAddWithoutValidation("X-API-Key", _apiKey);

            var customerId = User.FindFirst("CustomerId")?.Value;
            if (!string.IsNullOrEmpty(customerId))
                req.Headers.TryAddWithoutValidation("CustomerId", customerId);

            return req;
        }

        protected async Task<IActionResult> ProxyAsync(HttpRequestMessage req)
        {
            var response = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            var content  = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                StatusCode  = (int)response.StatusCode,
                ContentType = "application/json",
                Content     = content.Length > 0 ? content : "null"
            };
        }
    }
}
