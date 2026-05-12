using System;
using System.Collections.Generic;
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
            req.Content = await ReadBodyWithGeneratedName();
            return await ProxyAsync(req);
        }

        private async Task<StringContent> ReadBodyWithGeneratedName()
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var dict = string.IsNullOrWhiteSpace(body)
                ? new Dictionary<string, JsonElement>()
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? new Dictionary<string, JsonElement>();

            var cityPart = dict.TryGetValue("cityId", out var c) && c.ValueKind == JsonValueKind.String
                ? c.GetString()![..Math.Min(8, c.GetString()!.Length)]
                : "xxxx";

            var districtPart = dict.TryGetValue("districtId", out var d) && d.ValueKind == JsonValueKind.String
                ? d.GetString()![..Math.Min(8, d.GetString()!.Length)]
                : "xxxx";

            var name = $"LOC-{cityPart}-{districtPart}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            dict["name"] = JsonSerializer.SerializeToElement(name);

            return new StringContent(JsonSerializer.Serialize(dict), Encoding.UTF8, "application/json");
        }
    }
}
