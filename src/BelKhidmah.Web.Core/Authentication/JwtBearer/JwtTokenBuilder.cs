using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Runtime.Security;
using BelKhidmah.Authorization.Users;
using BelKhidmah.Models.TokenAuth;

namespace BelKhidmah.Authentication.JwtBearer
{
    public class JwtTokenBuilder : ITransientDependency
    {
        private const string CustomerIdClaimType = "CustomerId";
        private const string OtpAuthType = "OtpAuth";

        private readonly TokenAuthConfiguration _configuration;

        public JwtTokenBuilder(TokenAuthConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthenticateResultModel Build(User user, ClaimsIdentity identity, Guid? externalCustomerId)
        {
            var claims = CreateJwtClaims(identity);
            AddCustomerIdClaim(claims, externalCustomerId);
            var accessToken = CreateAccessToken(claims);

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = SimpleStringCipher.Instance.Encrypt(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = user.Id
            };
        }

        public ClaimsIdentity BuildIdentityForUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            if (user.TenantId.HasValue)
                claims.Add(new Claim(AbpClaimTypes.TenantId, user.TenantId.Value.ToString()));

            return new ClaimsIdentity(claims, OtpAuthType);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private static void AddCustomerIdClaim(List<Claim> claims, Guid? externalCustomerId)
        {
            if (externalCustomerId.HasValue)
                claims.Add(new Claim(CustomerIdClaimType, externalCustomerId.Value.ToString()));
        }

        private string CreateAccessToken(IEnumerable<Claim> claims)
        {
            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
