using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        public async Task<IActionResult> Get()
        {
            var response = await Client.SendAsync(BuildRequest(HttpMethod.Get, "api/customerlocations/Get"));
            var content  = await response.Content.ReadAsStringAsync();
            content = TransformLocationNames(content, isArray: true);
            return new ContentResult { StatusCode = (int)response.StatusCode, ContentType = "application/json", Content = content };
        }

        [HttpGet("GetDefault")]
        public async Task<IActionResult> GetDefault()
        {
            var response = await Client.SendAsync(BuildRequest(HttpMethod.Get, "api/customerlocations/GetDefault"));
            var content  = await response.Content.ReadAsStringAsync();
            content = TransformLocationNames(content, isArray: false);
            return new ContentResult { StatusCode = (int)response.StatusCode, ContentType = "application/json", Content = content };
        }

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

            // Build display name from city/district names
            var cityName     = dict.TryGetValue("cityName",     out var c) && c.ValueKind == JsonValueKind.String ? c.GetString()!.Trim() : string.Empty;
            var districtName = dict.TryGetValue("districtName", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString()!.Trim() : string.Empty;
            var displayName  = string.Join(" - ", new[] { cityName, districtName }.Where(p => !string.IsNullOrEmpty(p)));

            // Generate internal LOC-xxx name for storage
            var cityPart = dict.TryGetValue("cityId", out var ci) && ci.ValueKind == JsonValueKind.String
                ? ci.GetString()![..Math.Min(8, ci.GetString()!.Length)]
                : "xxxx";
            var districtPart = dict.TryGetValue("districtId", out var di) && di.ValueKind == JsonValueKind.String
                ? di.GetString()![..Math.Min(8, di.GetString()!.Length)]
                : "xxxx";

            dict["name"] = JsonSerializer.SerializeToElement($"LOC-{cityPart}-{districtPart}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

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

        private static string TransformLocationNames(string content, bool isArray)
        {
            if (string.IsNullOrEmpty(content)) return content;
            try
            {
                var json = JsonNode.Parse(content);
                var data = json?["data"];
                if (data == null) return content;

                if (isArray)
                {
                    foreach (var item in data.AsArray())
                        ApplyDisplayName(item);
                }
                else
                {
                    ApplyDisplayName(data);
                }

                return json.ToJsonString();
            }
            catch { return content; }
        }

        private static void ApplyDisplayName(JsonNode item)
        {
            if (item == null) return;
            var city     = item["cityName"]?.GetValue<string>()?.Trim() ?? string.Empty;
            var district = item["districtName"]?.GetValue<string>()?.Trim() ?? string.Empty;
            var display  = string.Join(" - ", new[] { city, district }.Where(p => !string.IsNullOrEmpty(p)));
            if (!string.IsNullOrEmpty(display))
                item["name"] = display;
        }
    }
}
