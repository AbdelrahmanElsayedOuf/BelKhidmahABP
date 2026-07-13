using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Abp;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.UI;
using BelKhidmah.Authentication.JwtBearer;
using BelKhidmah.Authorization;
using BelKhidmah.Authorization.Users;
using BelKhidmah.Customers;
using BelKhidmah.Models.TokenAuth;
using BelKhidmah.MultiTenancy;
using BelKhidmah.Otp;

namespace BelKhidmah.Authentication
{
    public class MobileAuthService : AbpServiceBase, ITransientDependency
    {
        private const int UnverifiedUserCleanupMinutes = 10;
        private const string RegistrationOtpTemplate = "RegisterationVerificationCode";
        private const string LoginOtpTemplate = "LoginCode";
        private const string SyntheticEmailDomain = "belkhidmah.local";

        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly OtpManager _otpManager;
        private readonly UserManager _userManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly ExternalCustomerService _externalCustomerService;
        private readonly JwtTokenBuilder _tokenBuilder;

        public IAbpSession AbpSession { get; set; } = NullAbpSession.Instance;

        public MobileAuthService(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            OtpManager otpManager,
            UserManager userManager,
            UserRegistrationManager userRegistrationManager,
            ExternalCustomerService externalCustomerService,
            JwtTokenBuilder tokenBuilder)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _otpManager = otpManager;
            _userManager = userManager;
            _userRegistrationManager = userRegistrationManager;
            _externalCustomerService = externalCustomerService;
            _tokenBuilder = tokenBuilder;
            LocalizationSourceName = BelKhidmahConsts.LocalizationSourceName;
        }

        // ---------- Public API ----------

        public async Task<AuthenticateResultModel> AuthenticateAsync(AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                GetTenancyNameOrNull());

            return _tokenBuilder.Build(loginResult.User, loginResult.Identity, loginResult.User.ExternalCustomerId);
        }

        public async Task<LoginResultDto> RegisterAsync(MobileRegisterInput model)
        {
            var deliveryMethod = await _otpManager.GetDeliveryMethodAsync();
            EnsureRegisterInputIsValid(model, deliveryMethod);

            var userName = model.PhoneNumber;
            var email    = model.EmailAddress ?? $"{model.PhoneNumber}@{SyntheticEmailDomain}";

            var conflicts = await FindConflictingUsersAsync(userName, model.EmailAddress);
            EnsureNoVerifiedConflict(conflicts);

            var reuseTarget = await ResolveReuseTargetAsync(conflicts);
            if (reuseTarget != null)
            {
                await ReuseUnverifiedUserAsync(reuseTarget, model, userName, email);
            }
            else
            {
                await CleanupStaleUnverifiedUsersAsync();
                await CreateUnverifiedUserAsync(model, userName, email);
            }

            return await SendRegistrationOtpAsync(model, deliveryMethod);
        }

        public async Task<LoginResultDto> ResendVerificationCodeAsync(SendCodeInput model)
        {
            var user = await FindUserByPhoneAsync(model.PhoneNumber)
                       ?? throw new UserFriendlyException("No account found for the provided phone number.");

            var deliveryMethod = await _otpManager.GetDeliveryMethodAsync();
            return await SendOtpForUserAsync(user, deliveryMethod, RegistrationOtpTemplate);
        }

        public async Task<LoginResultDto> LoginAsync(SendCodeInput model)
        {
            var user = await FindUserByPhoneAsync(model.PhoneNumber)
                       ?? throw new UserFriendlyException(300, L("UserMustRegisterFirst"));

            if (!user.IsActive)
                throw new UserFriendlyException(L("AccountIsNotActive"));

            var deliveryMethod = await _otpManager.GetDeliveryMethodAsync();

            if (!IsVerifiedFor(user, deliveryMethod))
            {
                var response = await SendOtpForUserAsync(user, deliveryMethod, RegistrationOtpTemplate);
                response.RequiresVerification = true;
                return response;
            }

            return await SendOtpForUserAsync(user, deliveryMethod, LoginOtpTemplate);
        }

        public async Task<AuthenticateResultModel> VerifyCodeAsync(VerifyCodeInput model)
        {
            await _otpManager.VerifyAsync(model.EmailOrPhone, model.Code);

            var user = await FindUserByAnyIdentifierAsync(model.EmailOrPhone)
                       ?? throw new UserFriendlyException("User not found.");

            var externalId = await EnsureExternalCustomerAsync(user);
            await MarkUserVerifiedAsync(user, externalId);

            return _tokenBuilder.Build(user, _tokenBuilder.BuildIdentityForUser(user), externalId);
        }

        // ---------- Registration helpers ----------

        private static void EnsureRegisterInputIsValid(MobileRegisterInput model, OtpDeliveryMethod deliveryMethod)
        {
            if (deliveryMethod == OtpDeliveryMethod.Sms && string.IsNullOrWhiteSpace(model.PhoneNumber))
                throw new UserFriendlyException("PhoneNumber is required.");
            if (deliveryMethod == OtpDeliveryMethod.Email && string.IsNullOrWhiteSpace(model.EmailAddress))
                throw new UserFriendlyException("EmailAddress is required.");
        }

        private async Task<(User ByName, User ByEmail)> FindConflictingUsersAsync(string userName, string emailAddress)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var byName = await _userManager.FindByNameAsync(userName);
                var byEmail = !string.IsNullOrWhiteSpace(emailAddress)
                    ? await _userManager.FindByEmailAsync(emailAddress)
                    : null;
                return (byName, byEmail);
            }
        }

        private static void EnsureNoVerifiedConflict((User ByName, User ByEmail) conflicts)
        {
            if (IsAccountVerified(conflicts.ByName) || IsAccountVerified(conflicts.ByEmail))
                throw new UserFriendlyException("A user with this email or phone already exists.");
        }

        // Two different unverified stubs each hold one of the incoming identifiers.
        // Drop the email-side stub so the phone-side row can be reused cleanly.
        private async Task<User> ResolveReuseTargetAsync((User ByName, User ByEmail) conflicts)
        {
            var (byName, byEmail) = conflicts;

            if (byName != null && byEmail != null && byName.Id != byEmail.Id)
            {
                await DeleteUnverifiedUserAsync(byEmail);
                byEmail = null;
            }

            return byName ?? byEmail;
        }

        private async Task ReuseUnverifiedUserAsync(User target, MobileRegisterInput model, string userName, string email)
        {
            await _otpManager.InvalidateActiveOtpsAsync(target.PhoneNumber, target.EmailAddress);

            target.Name = model.Name;
            target.Surname = model.Surname ?? model.Name;
            target.UserName = userName;
            target.EmailAddress = email;
            target.PhoneNumber = model.PhoneNumber;
            target.SetNormalizedNames();

            var result = await _userManager.UpdateAsync(target);
            if (!result.Succeeded)
                throw new UserFriendlyException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        private async Task<User> CreateUnverifiedUserAsync(MobileRegisterInput model, string userName, string email)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                model.Name,
                model.Surname ?? model.Name,
                email,
                userName,
                false);

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                user.PhoneNumber = model.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            return user;
        }

        private async Task CleanupStaleUnverifiedUsersAsync()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-UnverifiedUserCleanupMinutes);

            List<User> stale;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                stale = await _userManager.Users
                    .Where(u => !u.IsActive
                                && !u.IsEmailConfirmed
                                && !u.IsPhoneNumberConfirmed
                                && u.CreationTime < threshold)
                    .ToListAsync();
            }

            foreach (var user in stale)
                await DeleteUnverifiedUserAsync(user);
        }

        private async Task DeleteUnverifiedUserAsync(User user)
        {
            await _otpManager.InvalidateActiveOtpsAsync(user.PhoneNumber, user.EmailAddress);
            await _userManager.DeleteAsync(user);
        }

        private Task<LoginResultDto> SendRegistrationOtpAsync(MobileRegisterInput model, OtpDeliveryMethod deliveryMethod)
        {
            var stub = new User
            {
                PhoneNumber = model.PhoneNumber,
                EmailAddress = model.EmailAddress
            };
            return SendOtpForUserAsync(stub, deliveryMethod, RegistrationOtpTemplate);
        }

        // ---------- OTP + user lookup helpers ----------

        private async Task<User> FindUserByPhoneAsync(string phoneNumber)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                return await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        private async Task<User> FindUserByAnyIdentifierAsync(string identifier)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                return await _userManager.FindByNameAsync(identifier)
                       ?? await _userManager.FindByEmailAsync(identifier)
                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier);
        }

        // OTP is always keyed by phone so VerifyCode only ever needs phone.
        // When the channel is Email, deliver to the user's email address instead.
        private async Task<LoginResultDto> SendOtpForUserAsync(User user, OtpDeliveryMethod deliveryMethod, string template)
        {
            var deliverTo = deliveryMethod == OtpDeliveryMethod.Email ? user.EmailAddress : null;
            await _otpManager.SendAsync(user.PhoneNumber, template, deliverTo);

            return new LoginResultDto
            {
                Message = BuildOtpSentMessage(deliveryMethod, user.EmailAddress, user.PhoneNumber)
            };
        }

        private string BuildOtpSentMessage(OtpDeliveryMethod deliveryMethod, string emailAddress, string phoneNumber)
            => deliveryMethod == OtpDeliveryMethod.Email
                ? L("OtpSentToEmail", emailAddress)
                : L("OtpSentToPhone", phoneNumber);

        private static bool IsAccountVerified(User user)
            => user != null && (user.IsEmailConfirmed || user.IsPhoneNumberConfirmed);

        private static bool IsVerifiedFor(User user, OtpDeliveryMethod deliveryMethod)
            => deliveryMethod == OtpDeliveryMethod.Email
                ? user.IsEmailConfirmed
                : user.IsPhoneNumberConfirmed;

        // ---------- Verification helpers ----------

        private async Task<Guid> EnsureExternalCustomerAsync(User user)
        {
            var externalId = await _externalCustomerService.CreateIfNotExistsAsync(
                user.ExternalCustomerId,
                user.FullName,
                user.PhoneNumber,
                user.EmailAddress);

            if (!externalId.HasValue)
                throw new UserFriendlyException("Could not verify your account in the system. Please try again later.");

            return externalId.Value;
        }

        private async Task MarkUserVerifiedAsync(User user, Guid externalCustomerId)
        {
            var deliveryMethod = await _otpManager.GetDeliveryMethodAsync();
            if (deliveryMethod == OtpDeliveryMethod.Email)
                user.IsEmailConfirmed = true;
            else
                user.IsPhoneNumberConfirmed = true;

            user.IsActive = true;
            user.ExternalCustomerId = externalCustomerId;

            await _userManager.UpdateAsync(user);
        }

        // ---------- Password login helpers ----------

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            if (loginResult.Result == Abp.Authorization.AbpLoginResultType.Success)
                return loginResult;

            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
        }

        private string GetTenancyNameOrNull()
            => AbpSession.TenantId.HasValue
                ? _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName
                : null;
    }
}
