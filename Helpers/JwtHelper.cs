using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineStoreRestfulApi.Helpers
{
    public class JwtHelper
    {
        private readonly string _key;

        public JwtHelper(IConfiguration config)
        {
            _key = config["JwtSettings:Key"]!;
        }

        public string GenerateJwtToken(int userId)
        {
            var key = Encoding.UTF8.GetBytes(_key); 

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim("userId", userId.ToString())
            }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            //Test see same or not
            Console.WriteLine("[DEBUG] >>> GENERATED JWT Token:");
            Console.WriteLine(tokenString);

            return tokenHandler.WriteToken(token);

        }
    }
}
