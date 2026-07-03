using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InternMS.Api.Services.Token
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _key;
        private readonly int _expiryMinutes;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
            _issuer = _config["Jwt:Issuer"] ?? "internms";
            _audience = _config["Jwt:Audience"] ?? "internms-users";
            _key = _config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key configuration is missing");
            int.TryParse(_config["Jwt:AccessTokenMinutes"], out _expiryMinutes);
            if (_expiryMinutes <= 0) _expiryMinutes = 60 * 24;
        }

        /// <summary>Gets the configured access token expiration in minutes</summary>
        public int GetAccessTokenExpirationMinutes()
        {
            return int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "15");
        }

        public string CreateAccessToken(Guid userId, string email, IEnumerable<string> roles)
        {
            return CreateAccessToken(userId, email, null, null, roles);
        }

        public string CreateAccessToken(Guid userId, string email, string? firstName, string? lastName, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("id", userId.ToString()),
                new Claim("sub", userId.ToString())
            };

            // Add user name claims
            if (!string.IsNullOrEmpty(firstName))
                claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, firstName));
            
            if (!string.IsNullOrEmpty(lastName))
                claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, lastName));

            // Add full name claim for convenience
            if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
            {
                var fullName = $"{firstName} {lastName}".Trim();
                claims.Add(new Claim(JwtRegisteredClaimNames.Name, fullName));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenMinutes"]!));

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}