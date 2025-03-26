using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Domain.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fun_Funding.Application.Services.ExternalServices
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly IConfiguration _configuration;

        public TokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(User user, IList<string> userRoles)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName!),
        new Claim(ClaimTypes.Email, user.Email!),
        new Claim("jti", "JcmF8uj2ISveL5FvvNk4pnp8xrhINz8-1614225624"),
        new Claim("api_key", "JcmF8uj2ISveL5FvvNk4pnp8xrhINz8")
    };

            if (userRoles != null && userRoles.Count > 0) // Ensure userRoles is not null or empty
            {
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role)); // Add each role as a claim
                }
            }

            var securityToken = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.UtcNow.AddDays(5),
                claims: claims,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }

    }
}
