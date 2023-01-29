using AccountingBot.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AccountingBot
{
    /// <summary>
    /// Class for generate JWT
    /// </summary>
    public static class JwtHelper
    {
        public static string GenTokenkey(this LoginInfo loginInfo, JwtSettings jwtSettings)
        {
            // Get secret key
            var key = System.Text.Encoding.ASCII.GetBytes(jwtSettings.IssuerSigningKey);
            Guid Id = Guid.Empty;
            IEnumerable<Claim> claims = new Claim[] {
                    new Claim(ClaimTypes.NameIdentifier, loginInfo.UserName),
                    new Claim(ClaimTypes.Expiration, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()) };

            DateTime expireTime = DateTime.UtcNow.AddDays(jwtSettings.ExpireDays);

            var JWToken = new JwtSecurityToken(issuer: jwtSettings.ValidIssuer, audience: jwtSettings.ValidAudience, claims, notBefore: DateTime.UtcNow, expires: expireTime, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(JWToken);
        }
    }
}
