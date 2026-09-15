using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.UI;
using BelKhidmah.Authorization.Users;

namespace BelKhidmah.Authentication.Biometric
{
    public class BiometricManager : DomainService
    {
        private const int NonceByteLength = 32;
        private const int ChallengeExpirySeconds = 60;

        private readonly IRepository<UserBiometricKey, Guid> _keyRepository;
        private readonly IRepository<BiometricChallenge, Guid> _challengeRepository;
        private readonly IRepository<User, long> _userRepository;

        public BiometricManager(
            IRepository<UserBiometricKey, Guid> keyRepository,
            IRepository<BiometricChallenge, Guid> challengeRepository,
            IRepository<User, long> userRepository)
        {
            _keyRepository = keyRepository;
            _challengeRepository = challengeRepository;
            _userRepository = userRepository;
            LocalizationSourceName = BelKhidmahConsts.LocalizationSourceName;
        }

        [UnitOfWork]
        public virtual async Task<Guid> EnrollAsync(long userId, string deviceId, string deviceName, string publicKeyBase64, string keyAlgorithm)
        {
            if (userId <= 0) throw new UserFriendlyException("User not authenticated.");
            if (string.IsNullOrWhiteSpace(deviceId)) throw new UserFriendlyException("DeviceId is required.");
            if (string.IsNullOrWhiteSpace(publicKeyBase64)) throw new UserFriendlyException("PublicKey is required.");

            var algorithm = string.IsNullOrWhiteSpace(keyAlgorithm) ? UserBiometricKey.AlgorithmEs256 : keyAlgorithm;
            if (!string.Equals(algorithm, UserBiometricKey.AlgorithmEs256, StringComparison.OrdinalIgnoreCase))
                throw new UserFriendlyException($"Unsupported key algorithm '{algorithm}'. Only ES256 is supported.");

            ValidatePublicKeyOrThrow(publicKeyBase64);

            var existing = await _keyRepository.FirstOrDefaultAsync(k =>
                k.UserId == userId && k.DeviceId == deviceId && !k.IsDeleted);

            if (existing != null)
            {
                existing.IsDeleted = true;
                await _keyRepository.UpdateAsync(existing);
            }

            var record = new UserBiometricKey
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceId = deviceId,
                DeviceName = deviceName,
                PublicKey = publicKeyBase64,
                KeyAlgorithm = UserBiometricKey.AlgorithmEs256,
                CreationTime = DateTime.UtcNow
            };

            await _keyRepository.InsertAsync(record);
            return record.Id;
        }

        [UnitOfWork]
        public virtual async Task<BiometricChallengeIssueResult> IssueChallengeAsync(Guid keyId)
        {
            var key = await _keyRepository.FirstOrDefaultAsync(k => k.Id == keyId && !k.IsDeleted)
                       ?? throw new UserFriendlyException(
                           (int)BiometricErrorCode.BiometricKeyNotFound,
                           L("Biometric_KeyNotFound"));

            var nonceBytes = RandomNumberGenerator.GetBytes(NonceByteLength);
            var nonce = Convert.ToBase64String(nonceBytes);

            var challenge = new BiometricChallenge
            {
                Id = Guid.NewGuid(),
                KeyId = key.Id,
                Nonce = nonce,
                ExpiresAt = DateTime.UtcNow.AddSeconds(ChallengeExpirySeconds),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _challengeRepository.InsertAsync(challenge);

            return new BiometricChallengeIssueResult
            {
                KeyId = key.Id,
                Nonce = nonce,
                ExpiresInSeconds = ChallengeExpirySeconds
            };
        }

        [UnitOfWork]
        public virtual async Task<User> VerifyAsync(Guid keyId, string nonceBase64, string signatureBase64)
        {
            if (string.IsNullOrWhiteSpace(nonceBase64))
                throw new UserFriendlyException(
                    (int)BiometricErrorCode.NonceRequired,
                    L("Biometric_NonceRequired"));

            if (string.IsNullOrWhiteSpace(signatureBase64))
                throw new UserFriendlyException(
                    (int)BiometricErrorCode.SignatureRequired,
                    L("Biometric_SignatureRequired"));

            var key = await _keyRepository.FirstOrDefaultAsync(k => k.Id == keyId && !k.IsDeleted)
                       ?? throw new UserFriendlyException(
                           (int)BiometricErrorCode.BiometricKeyNotFound,
                           L("Biometric_KeyNotFound"));

            var challenge = await _challengeRepository.FirstOrDefaultAsync(c =>
                c.KeyId == keyId && c.Nonce == nonceBase64 && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
                            ?? throw new UserFriendlyException(
                                (int)BiometricErrorCode.InvalidOrExpiredChallenge,
                                L("Biometric_InvalidOrExpiredChallenge"));

            if (!VerifySignature(key.PublicKey, nonceBase64, signatureBase64))
                throw new UserFriendlyException(
                    (int)BiometricErrorCode.SignatureVerificationFailed,
                    L("Biometric_SignatureVerificationFailed"));

            challenge.IsUsed = true;
            await _challengeRepository.UpdateAsync(challenge);

            key.LastUsedAt = DateTime.UtcNow;
            await _keyRepository.UpdateAsync(key);

            User user;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                user = await _userRepository.FirstOrDefaultAsync(key.UserId);
            }

            if (user == null || !user.IsActive)
                throw new UserFriendlyException(
                    (int)BiometricErrorCode.AccountNotActive,
                    L("Biometric_AccountNotActive"));

            return user;
        }

        [UnitOfWork]
        public virtual async Task<List<UserBiometricKey>> ListDevicesAsync(long userId)
        {
            return await _keyRepository.GetAllListAsync(k => k.UserId == userId && !k.IsDeleted);
        }

        [UnitOfWork]
        public virtual async Task RevokeAsync(long userId, Guid keyId)
        {
            var key = await _keyRepository.FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId && !k.IsDeleted)
                       ?? throw new UserFriendlyException("Biometric key not found.");

            key.IsDeleted = true;
            await _keyRepository.UpdateAsync(key);
        }

        private static void ValidatePublicKeyOrThrow(string publicKeyBase64)
        {
            byte[] keyBytes;
            try
            {
                keyBytes = Convert.FromBase64String(publicKeyBase64);
            }
            catch (FormatException)
            {
                throw new UserFriendlyException("PublicKey must be base64-encoded SubjectPublicKeyInfo.");
            }

            try
            {
                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

                var parameters = ecdsa.ExportParameters(false);
                if (parameters.Curve.Oid?.Value != ECCurve.NamedCurves.nistP256.Oid.Value)
                    throw new UserFriendlyException("PublicKey must be an ECDSA P-256 key.");
            }
            catch (CryptographicException)
            {
                throw new UserFriendlyException("PublicKey is not a valid ECDSA public key.");
            }
        }

        private static bool VerifySignature(string publicKeyBase64, string nonceBase64, string signatureBase64)
        {
            byte[] keyBytes, nonceBytes, sigBytes;
            try
            {
                keyBytes = Convert.FromBase64String(publicKeyBase64);
                nonceBytes = Convert.FromBase64String(nonceBase64);
                sigBytes = Convert.FromBase64String(signatureBase64);
            }
            catch (FormatException)
            {
                return false;
            }

            try
            {
                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                return ecdsa.VerifyData(
                    nonceBytes,
                    sigBytes,
                    HashAlgorithmName.SHA256,
                    DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }

    public class BiometricChallengeIssueResult
    {
        public Guid KeyId { get; set; }
        public string Nonce { get; set; }
        public int ExpiresInSeconds { get; set; }
    }
}
