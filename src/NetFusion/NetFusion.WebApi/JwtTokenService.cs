using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.WebApi.Configs;
using NetFusion.WebApi.Modules;
using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Security.Tokens;
using System.Threading;

namespace NetFusion.WebApi
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IClaimTokenModule _claimTokenModule;
        private readonly JwtTokenSettings _tokenSettings;

        public JwtTokenService(
            IClaimTokenModule claimTokenModule,
            JwtTokenSettings tokenSettings)
        {
            _claimTokenModule = claimTokenModule;
            _tokenSettings = tokenSettings;
        }

        public string CreateSecurityToken(object claimObj)
        {
            byte[] key = Convert.FromBase64String(_tokenSettings.JwtKey);

            var signingCredentials = new SigningCredentials(
                new InMemorySymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature,
                SecurityAlgorithms.Sha256Digest);

            var descriptor = CreateTokenDescriptor(signingCredentials);
            descriptor.Subject = CreateClaimIdentity(claimObj);

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }

        private SecurityTokenDescriptor CreateTokenDescriptor(SigningCredentials signingCredentials)
        {
            var now = DateTime.UtcNow;
            var descriptor = new SecurityTokenDescriptor()
            {
                TokenIssuerName = _tokenSettings.TokenIssuerName,
                AppliesToAddress = _tokenSettings.AppliesToAddress,
                Lifetime = new Lifetime(now, now.AddMinutes(_tokenSettings.TokenTimeoutMinutes)),
                SigningCredentials = signingCredentials
            };
            return descriptor;
        }

        private ClaimsIdentity CreateClaimIdentity(object claimObj)
        {
            var claims = GetClaimsValues(claimObj);
            return new ClaimsIdentity(claims);
        }

        private IEnumerable<Claim> GetClaimsValues(object claimObj)
        {
            var claims = claimObj.ToDictionary()
                .Select(kv => new Claim(kv.Key, CheckForArrayValue(kv.Value)));

            AssertUniqueKeys(claims);
            return claims;
        }

        private string CheckForArrayValue(object value)
        {
            if (value.GetType().IsArray)
            {
                var values = value as object[];
                if (values != null)
                {
                    return String.Join(",", values);
                }
            }
            return value.ToString();
        }

        private void AssertUniqueKeys(IEnumerable<Claim> claims)
        {
            var duplicateClaims = claims.GroupBy(c => c.Type)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            if (duplicateClaims.Any())
            {
                throw new InvalidOperationException(
                    $"The following claim types are duplicated: {String.Join(", ", duplicateClaims) }");
            }
        }

        private ClaimsPrincipal CreatePrincipalFromToken(string jwtToken)
        {
            SecurityToken token = null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var parms = new TokenValidationParameters
            {
                ValidIssuer = _tokenSettings.TokenIssuerName,
                ValidAudience = _tokenSettings.AppliesToAddress,
                ValidateIssuer = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                IssuerSigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(_tokenSettings.JwtKey)),
            };

            return tokenHandler.ValidateToken(jwtToken, parms, out token);
        }


        public TPrincipal CreatePrincipal<TPrincipal>(string jwtToken)
            where TPrincipal : FusionPrincipal
        {
            Check.NotNullOrWhiteSpace(jwtToken, nameof(jwtToken));

            var claimsPrincipal = CreatePrincipalFromToken(jwtToken);
            var hostPrincipal =  TypeExtensions.CreateInstance<TPrincipal>(claimsPrincipal);

            Thread.CurrentPrincipal = hostPrincipal;
            return hostPrincipal;
        }

        public FusionPrincipal CreatePrincipal(string jwtToken)
        {
            Check.NotNullOrWhiteSpace(jwtToken, nameof(jwtToken));

            var claimsPrincipal = CreatePrincipalFromToken(jwtToken);
            var hostPrincipal = (FusionPrincipal)Activator.CreateInstance(_claimTokenModule.ApplicationPrincipalType, claimsPrincipal);

            Thread.CurrentPrincipal = hostPrincipal;
            return hostPrincipal;
        }

    }
}
