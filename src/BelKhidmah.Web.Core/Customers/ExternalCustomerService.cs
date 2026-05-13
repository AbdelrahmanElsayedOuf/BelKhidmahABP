using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Abp.Dependency;
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

            const int maxAttempts = 3;
            HttpResponseMessage response = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "en/api/Customers/Create")
                    {
                        Content = JsonContent.Create(new { Name = name, Phone = phone, Email = email })
                    };

                    if (!string.IsNullOrEmpty(_apiKey))
                        request.Headers.TryAddWithoutValidation("X-API-Key", _apiKey);

                    response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                        break;

                    Logger.WarnFormat("[ExternalCustomer] Create failed (attempt {0}/{1}). Status={2}", attempt, maxAttempts, (int)response.StatusCode);
                }
                catch (Exception ex)
                {
                    Logger.WarnFormat("[ExternalCustomer] Create threw on attempt {0}/{1}: {2}", attempt, maxAttempts, ex.Message);
                }
            }

            if (response == null || !response.IsSuccessStatusCode)
                return null;

            var x = await response.Content.ReadAsStringAsync();
            var result = await response.Content.ReadFromJsonAsync<ExternalCreateResponse>();

            if (result?.Success != true || result.Data == Guid.Empty)
            {
                Logger.WarnFormat("[ExternalCustomer] Response indicated failure or missing Id.");
                return null;
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
