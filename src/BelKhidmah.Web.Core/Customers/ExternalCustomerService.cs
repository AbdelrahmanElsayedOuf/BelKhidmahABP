using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.UI;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Customers
{
    public class ExternalCustomerService : ITransientDependency
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public ExternalCustomerService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["ExternalApi:ApiKey"];
        }

        public async Task<Guid?> CreateIfNotExistsAsync(Guid? existingId, string name, string phone, string email)
        {
            if (existingId.HasValue)
                return existingId;

            var client = _httpClientFactory.CreateClient("ExternalApi");

            var request = new HttpRequestMessage(HttpMethod.Post, "en/api/BelkhidmahCustomers/Create")
            {
                Content = JsonContent.Create(new { Name = name, Phone = phone, Email = email })
            };

            if (!string.IsNullOrEmpty(_apiKey))
                request.Headers.TryAddWithoutValidation("X-API-Key", _apiKey);

            var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // TEMP: surface the full readable response through VerifyCode (no logging access).
                throw new UserFriendlyException(
                    $"[ExternalCustomer] Create failed. Status={(int)response.StatusCode}, Body={body}");
            }

            var result = JsonSerializer.Deserialize<ExternalCreateResponse>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Success != true || result.Data == Guid.Empty)
            {
                // TEMP: surface the full readable response through VerifyCode (no logging access).
                throw new UserFriendlyException($"[ExternalCustomer] Response indicated failure or missing Id. Body={body}");
            }

            return result.Data;
        }

        private class ExternalCreateResponse
        {
            public bool Success { get; set; }
            public Guid Data { get; set; }
        }
    }
}
