using Abp.Logging;
using BelKhidmah.Authorization.Users;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BelKhidmah.Controllers
{
    [Authorize]
    [Route("api/profile")]
    public class ProfileController : BelKhidmahProxyControllerBase
    {
        private readonly UserManager _userManager;
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public ProfileController(
            IHttpClientFactory factory,
            IConfiguration configuration,
            UserManager userManager) : base(factory, configuration)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public Task<IActionResult> Get()
            => ProxyAsync(BuildRequest(HttpMethod.Get, "api/Profile"));

        [HttpPut]
        public async Task<IActionResult> Update()
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var req = BuildRequest(HttpMethod.Put, "api/Profile");
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var result = await ProxyAsync(req);

            if (AbpSession.UserId.HasValue && !string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
                    var json = JsonDocument.Parse(body).RootElement;

                    if (json.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
                        user.Name = name.GetString();

                    if (json.TryGetProperty("email", out var email) && email.ValueKind == JsonValueKind.String)
                        user.EmailAddress = email.GetString();

                    if (json.TryGetProperty("phone", out var phone) && phone.ValueKind == JsonValueKind.String)
                        user.PhoneNumber = phone.GetString();

                    await _userManager.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    Logger.WarnFormat("[ProfileController] Failed to update ABP user {0}: {1}", AbpSession.UserId, ex.Message);
                }
            }

            return result;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var result = await ProxyAsync(BuildRequest(HttpMethod.Delete, "api/Profile"));

            if (AbpSession.UserId.HasValue)
            {
                try
                {
                    var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    Logger.WarnFormat("[ProfileController] Failed to deactivate ABP user {0}: {1}", AbpSession.UserId, ex.Message);
                }
            }

            return result;
        }
    }
}
