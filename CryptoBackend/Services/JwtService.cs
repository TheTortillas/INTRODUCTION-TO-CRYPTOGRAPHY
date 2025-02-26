using CryptoBackend.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CryptoBackend.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateToken(UserDTO user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWTSettings:securityKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", user.ID.ToString()),
                    new Claim("firstname", user.Firstname),
                    new Claim("lastname", user.Lastname),
                    new Claim("secondlastname", user.SecondLastname),
                    new Claim("email", user.Email),
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["JWTSettings:expiryInMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["JWTSettings:validIssuer"],
                Audience = _config["JWTSettings:validAudience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public string RefreshToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWTSettings:securityKey"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["JWTSettings:validIssuer"],
                    ValidAudience = _config["JWTSettings:validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Evita márgenes de tiempo
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var id = jwtToken.Claims.First(x => x.Type == "id").Value;
                var firstname = jwtToken.Claims.First(x => x.Type == "firstname").Value;
                var lastname = jwtToken.Claims.First(x => x.Type == "lastname").Value;
                var secondlastname = jwtToken.Claims.First(x => x.Type == "secondlastname").Value;
                var email = jwtToken.Claims.First(x => x.Type == "email").Value;

                var user = new UserDTO
                {
                    ID = int.Parse(id),
                    Firstname = firstname,
                    Lastname = lastname,
                    SecondLastname = secondlastname,
                    Email = email
                };

                return CreateToken(user);
            }
            catch
            {
                throw new SecurityTokenException("Token inválido o expirado.");
            }
        }
    }
}
