using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.UI;
using BelKhidmah.Authorization.Users;
using BelKhidmah.Configuration;

namespace BelKhidmah.Otp
{
    public class OtpManager : DomainService
    {
        private static readonly Random _rng = new Random();
        private const int ExpiryMinutes = 5;

        private readonly IRepository<OtpCode, Guid> _otpRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IEmailOtpSender _emailSender;
        private readonly ISmsOtpSender _smsSender;
        private readonly ISettingManager _settingManager;

        public OtpManager(
            IRepository<OtpCode, Guid> otpRepository,
            IRepository<User, long> userRepository,
            IEmailOtpSender emailSender,
            ISmsOtpSender smsSender,
            ISettingManager settingManager)
        {
            _otpRepository = otpRepository;
            _userRepository = userRepository;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _settingManager = settingManager;
        }

        public async Task SendAsync(string storageKey, string template = null, string deliverTo = null)
        {
            var code = _rng.Next(1000, 10000).ToString();

            await _otpRepository.InsertAsync(new OtpCode
            {
                Id = Guid.NewGuid(),
                EmailOrPhone = storageKey.ToLowerInvariant(),
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ExpiryMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            });

            var methodValue = await _settingManager.GetSettingValueAsync(AppSettingNames.OtpDeliveryMethod);
            var method = Enum.TryParse<OtpDeliveryMethod>(methodValue, true, out var parsed) ? parsed : OtpDeliveryMethod.Email;

            IOtpSender sender = method == OtpDeliveryMethod.Sms ? _smsSender : _emailSender;
            try
            {
                await sender.SendAsync(deliverTo ?? storageKey, code, template);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to send OTP; continuing without throwing.", ex);
            }
        }

        public async Task<OtpDeliveryMethod> GetDeliveryMethodAsync()
        {
            var value = await _settingManager.GetSettingValueAsync(AppSettingNames.OtpDeliveryMethod);
            return Enum.TryParse<OtpDeliveryMethod>(value, true, out var parsed) ? parsed : OtpDeliveryMethod.Email;
        }

        public async Task<bool> VerifyAsync(string emailOrPhone, string code)
        {
            var allowSpecific = await _settingManager.GetSettingValueAsync<bool>(AppSettingNames.AllowSpecificOtpCode);
            if (allowSpecific)
            {
                var specificCode = await _settingManager.GetSettingValueAsync(AppSettingNames.SpecificOtpCode);
                if (!string.IsNullOrWhiteSpace(specificCode) && code == specificCode)
                    return true;
            }

            var record = (await _otpRepository.GetAllListAsync(o =>
                o.EmailOrPhone == emailOrPhone.ToLowerInvariant() &&
                o.Code == code &&
                !o.IsUsed &&
                o.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (record == null)
                throw new UserFriendlyException("Invalid or expired code.");

            record.IsUsed = true;
            await _otpRepository.UpdateAsync(record);
            return true;
        }
    }
}
