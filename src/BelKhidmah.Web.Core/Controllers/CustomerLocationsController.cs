using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet("GetDefault")]
        public Task<IActionResult> GetDefault()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/customerlocations/GetDefault"));

        [HttpPost("Create")]
        public async Task<IActionResult> Create()
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();

            var dict = string.IsNullOrWhiteSpace(rawBody)
                ? new Dictionary<string, JsonElement>()
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rawBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? new Dictionary<string, JsonElement>();

            // Generate unique external location name for storage
            var cityPart = dict.TryGetValue("cityId", out var ci) && ci.ValueKind == JsonValueKind.String
                ? ci.GetString()![..Math.Min(8, ci.GetString()!.Length)]
                : "xxxx";
            var districtPart = dict.TryGetValue("districtId", out var di) && di.ValueKind == JsonValueKind.String
                ? di.GetString()![..Math.Min(8, di.GetString()!.Length)]
                : "xxxx";

            dict["externalLocationName"] = JsonSerializer.SerializeToElement($"LOC-{cityPart}-{districtPart}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

            var req = BuildRequest(HttpMethod.Post, "api/customerlocations/Create");
            req.Content = new StringContent(JsonSerializer.Serialize(dict), Encoding.UTF8, "application/json");

            var response = await Client.SendAsync(req);
            var content  = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                StatusCode  = (int)response.StatusCode,
                ContentType = "application/json",
                Content     = content
            };
        }

    }
}
