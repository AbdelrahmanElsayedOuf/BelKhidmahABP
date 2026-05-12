using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using BelKhidmah.Otp;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Otp
{
    public class BelKhidmahSmsOtpSender : ISmsOtpSender, ITransientDependency
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public BelKhidmahSmsOtpSender(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["ExternalApi:ApiKey"];
        }

        public async Task SendAsync(string recipient, string code, string template = null)
        {
            var client = _httpClientFactory.CreateClient("ExternalApi");

            var request = new HttpRequestMessage(HttpMethod.Post, "en/Sms/Send")
            {
                Content = JsonContent.Create(new
                {
                    PhoneNumber = recipient,
                    Template = template ?? "RegisterationVerificationCode",
                    Properties = new Dictionary<string, string> { { "OTP", code } }
                })
            };

            if (!string.IsNullOrEmpty(_apiKey))
                request.Headers.TryAddWithoutValidation("X-API-Key", _apiKey);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Logger.WarnFormat("[OTP-SMS] Send failed. Recipient={0} Status={1} Body={2}", recipient, (int)response.StatusCode, body);
            }
        }
    }
}
