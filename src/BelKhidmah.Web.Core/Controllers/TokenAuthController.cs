using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.UI;
using BelKhidmah.Authentication.JwtBearer;
using BelKhidmah.Authorization;
using BelKhidmah.Authorization.Users;
using BelKhidmah.Customers;
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
        private readonly ExternalCustomerService _externalCustomerService;

        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            OtpManager otpManager,
            UserManager userManager,
            UserRegistrationManager userRegistrationManager,
            ExternalCustomerService externalCustomerService)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _otpManager = otpManager;
            _userManager = userManager;
            _userRegistrationManager = userRegistrationManager;
            _externalCustomerService = externalCustomerService;
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                GetTenancyNameOrNull()
            );

            var claims   = CreateJwtClaims(loginResult.Identity);
            AddCustomerIdClaim(claims, loginResult.User.ExternalCustomerId);
            var accessToken = CreateAccessToken(claims);

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }

        [HttpPost]
        public async Task<LoginResultDto> Register([FromBody] MobileRegisterInput model)
        {
            var deliveryMethod = await _otpManager.GetDeliveryMethodAsync();

            if (deliveryMethod == OtpDeliveryMethod.Sms && string.IsNullOrWhiteSpace(model.PhoneNumber))
                throw new UserFriendlyException("PhoneNumber is required.");
            if (deliveryMethod == OtpDeliveryMethod.Email && string.IsNullOrWhiteSpace(model.EmailAddress))
                throw new UserFriendlyException("EmailAddress is required.");

            var userName = model.PhoneNumber;
            var email    = model.EmailAddress ?? $"{model.PhoneNumber}@belkhidmah.local";

            var existingUser = await _userManager.FindByNameAsync(userName);
            if (existingUser != null)
                throw new UserFriendlyException("A user with this email or phone already exists.");

            if (!string.IsNullOrWhiteSpace(model.EmailAddress))
            {
                var existingEmail = await _userManager.FindByEmailAsync(model.EmailAddress);
                if (existingEmail != null)
                    throw new UserFriendlyException("A user with this email already exists.");
            }

            var user = await _userRegistrationManager.RegisterAsync(
                model.Name,
                model.Surname ?? model.Name,
                email,
                userName,
                false
            );

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                user.PhoneNumber = model.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            var deliverTo = deliveryMethod == OtpDeliveryMethod.Email ? model.EmailAddress : null;

            await _otpManager.SendAsync(model.PhoneNumber, "RegisterationVerificationCode", deliverTo);

            var message = deliveryMethod == OtpDeliveryMethod.Email
                ? L("OtpSentToEmail", model.EmailAddress)
                : L("OtpSentToPhone", model.PhoneNumber);

            return new LoginResultDto { Message = message };
        }

        [HttpPost]
        public async Task<LoginResultDto> ResendVerificationCode([FromBody] SendCodeInput model)
        {
            User user;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (user == null)
                throw new UserFriendlyException("No account found for the provided phone number.");

            var deliveryMethod = await _otpManager.GetDeliveryMethodAsync();

            var isVerified = deliveryMethod == OtpDeliveryMethod.Email
                ? user.IsEmailConfirmed
                : user.IsPhoneNumberConfirmed;

            var deliverTo = deliveryMethod == OtpDeliveryMethod.Email ? user.EmailAddress : null;

            await _otpManager.SendAsync(user.PhoneNumber, "RegisterationVerificationCode", deliverTo);

            var message = deliveryMethod == OtpDeliveryMethod.Email
                ? L("OtpSentToEmail", user.EmailAddress)
                : L("OtpSentToPhone", user.PhoneNumber);

            return new LoginResultDto { Message = message };
        }

        [HttpPost]
        public async Task<LoginResultDto> Login([FromBody] SendCodeInput model)
        {
            User user;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (user == null)
                throw new UserFriendlyException(300, L("UserMustRegisterFirst"));

            var deliveryMethod = await _otpManager.GetDeliveryMethodAsync();

            var isVerified = deliveryMethod == OtpDeliveryMethod.Email
                ? user.IsEmailConfirmed
                : user.IsPhoneNumberConfirmed;

            if (!user.IsActive)
                throw new UserFriendlyException(L("AccountIsNotActive"));

            // OTP always keyed by phone so VerifyCode only ever needs phone.
            // When channel is Email, deliver to the user's email address instead.
            var deliverTo = deliveryMethod == OtpDeliveryMethod.Email ? user.EmailAddress : null;

            if (!isVerified)
            {
                await _otpManager.SendAsync(user.PhoneNumber, "RegisterationVerificationCode", deliverTo);
                var verifyMessage = deliveryMethod == OtpDeliveryMethod.Email
                    ? L("OtpSentToEmail", user.EmailAddress)
                    : L("OtpSentToPhone", user.PhoneNumber);
                return new LoginResultDto { Message = verifyMessage, RequiresVerification = true };
            }

            await _otpManager.SendAsync(user.PhoneNumber, "LoginCode", deliverTo);

            var message = deliveryMethod == OtpDeliveryMethod.Email
                ? L("OtpSentToEmail", user.EmailAddress)
                : L("OtpSentToPhone", user.PhoneNumber);

            return new LoginResultDto { Message = message };
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> VerifyCode([FromBody] VerifyCodeInput model)
        {
            await _otpManager.VerifyAsync(model.EmailOrPhone, model.Code);

            User user;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                user = await _userManager.FindByNameAsync(model.EmailOrPhone)
                       ?? await _userManager.FindByEmailAsync(model.EmailOrPhone)
                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.EmailOrPhone);

            if (user == null)
                throw new UserFriendlyException("User not found.");

            var externalId = await _externalCustomerService.CreateIfNotExistsAsync(
                user.ExternalCustomerId,
                user.FullName,
                user.PhoneNumber,
                user.EmailAddress
            );

            if (!externalId.HasValue)
                throw new UserFriendlyException("Could not verify your account in the system. Please try again later.");

            var deliveryMethod = await _otpManager.GetDeliveryMethodAsync();
            if (deliveryMethod == OtpDeliveryMethod.Email)
                user.IsEmailConfirmed = true;
            else
                user.IsPhoneNumberConfirmed = true;

            user.IsActive = true;
            user.ExternalCustomerId = externalId;

            await _userManager.UpdateAsync(user);

            var identity   = BuildIdentityForUser(user);
            var claims     = CreateJwtClaims(identity);
            AddCustomerIdClaim(claims, externalId);
            var accessToken = CreateAccessToken(claims);

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = user.Id
            };
        }

        private static void AddCustomerIdClaim(List<Claim> claims, Guid? externalCustomerId)
        {
            if (externalCustomerId.HasValue)
                claims.Add(new Claim("CustomerId", externalCustomerId.Value.ToString()));
        }

        private static ClaimsIdentity BuildIdentityForUser(Authorization.Users.User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            if (user.TenantId.HasValue)
                claims.Add(new Claim(AbpClaimTypes.TenantId, user.TenantId.Value.ToString()));

            return new ClaimsIdentity(claims, "OtpAuth");
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
                return null;

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
            var claims      = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

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
