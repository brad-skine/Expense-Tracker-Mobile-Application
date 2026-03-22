using expense_tracker.Models;
using expense_tracker.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace expense_tracker.Services
{

        public class TokenService : ITokenService
        {
            private readonly RsaSecurityKey _privateKey;
            private readonly IConfiguration _config;

            public TokenService(IConfiguration config)
            {
                _config = config;
                var privateKeyPem = File.ReadAllText("Utils/Keys/jwt_private.pem");
                var rsa = RSA.Create();
                rsa.ImportFromPem(privateKeyPem);
                _privateKey = new RsaSecurityKey(rsa);
            }
            public string GenerateToken(User user)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };
                
                var credentials= new SigningCredentials(
                    _privateKey,
            SecurityAlgorithms.RsaSha256);
                
                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: credentials
                    );
            
                return new JwtSecurityTokenHandler().WriteToken(token);

            }


        }
    }

