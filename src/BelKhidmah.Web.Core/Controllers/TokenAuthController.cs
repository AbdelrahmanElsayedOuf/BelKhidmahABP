using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.UI;
using BelKhidmah.Authentication.JwtBearer;
using BelKhidmah.Authorization;
using BelKhidmah.Authorization.Users;
using BelKhidmah.Models.TokenAuth;
using BelKhidmah.MultiTenancy;
using BelKhidmah.Otp;

namespace BelKhidmah.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : BelKhidmahControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly OtpManager _otpManager;
        private readonly UserManager _userManager;
        private readonly UserRegistrationManager _userRegistrationManager;

        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            OtpManager otpManager,
            UserManager userManager,
            UserRegistrationManager userRegistrationManager)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _otpManager = otpManager;
            _userManager = userManager;
            _userRegistrationManager = userRegistrationManager;
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                GetTenancyNameOrNull()
            );

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }

        [HttpPost]
        public async Task<bool> Register([FromBody] MobileRegisterInput model)
        {
            if (string.IsNullOrWhiteSpace(model.EmailAddress) && string.IsNullOrWhiteSpace(model.PhoneNumber))
                throw new UserFriendlyException("EmailAddress or PhoneNumber is required.");

            var userName = model.EmailAddress ?? model.PhoneNumber;
            var email = model.EmailAddress ?? $"{model.PhoneNumber}@belkhidmah.local";

            var existingUser = await _userManager.FindByNameAsync(userName);
            if (existingUser != null)
                throw new UserFriendlyException("A user with this email or phone already exists.");

            var user = await _userRegistrationManager.RegisterAsync(
                model.Name,
                model.Surname ?? model.Name,
                email,
                userName,
                true
            );

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                user.PhoneNumber = model.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            return true;
        }

        [HttpPost]
        public async Task<bool> SendCode([FromBody] SendCodeInput model)
        {
            var user = await _userManager.FindByNameAsync(model.EmailOrPhone)
                       ?? await _userManager.FindByEmailAsync(model.EmailOrPhone);

            if (user == null)
                throw new UserFriendlyException("No account found for the provided email or phone.");

            await _otpManager.SendAsync(model.EmailOrPhone);
            return true;
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> VerifyCode([FromBody] VerifyCodeInput model)
        {
            await _otpManager.VerifyAsync(model.EmailOrPhone, model.Code);

            var user = await _userManager.FindByNameAsync(model.EmailOrPhone)
                       ?? await _userManager.FindByEmailAsync(model.EmailOrPhone);

            if (user == null)
                throw new UserFriendlyException("User not found.");

            var identity = BuildIdentityForUser(user);
            var accessToken = CreateAccessToken(CreateJwtClaims(identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = user.Id
            };
        }

        private static System.Security.Claims.ClaimsIdentity BuildIdentityForUser(Authorization.Users.User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            if (user.TenantId.HasValue)
                claims.Add(new Claim(Abp.Runtime.Security.AbpClaimTypes.TenantId, user.TenantId.Value.ToString()));

            return new System.Security.Claims.ClaimsIdentity(claims, "OtpAuth");
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken);
        }
    }
}
