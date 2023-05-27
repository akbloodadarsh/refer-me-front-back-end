using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using static refer_me_back_end.Services.CosmosService.UserCosmosService;

namespace refer_me_back_end.Services.JWTAuthService
{
    public interface IJwtTokenManager
    {
        Task<string> Authenticate(string userName, string password);
    }

    public class JwtTokenManager : IJwtTokenManager
    {
        private readonly IUserCosmosDbService _usersCosmosDbService;
        private IConfiguration _configuration;
        public JwtTokenManager(IUserCosmosDbService usersCosmosDbService, IConfiguration configuration)
        {
            _usersCosmosDbService = usersCosmosDbService;
            _configuration = configuration;
        }

        public Task<string> Authenticate(string userName, string password)
        {
            var response = _usersCosmosDbService.AuthenticateUserAsync(userName, password).Result;

            if (response == HttpStatusCode.Unauthorized)
                return Task.FromResult("Username or password is incorrect");
            if (response == HttpStatusCode.NotFound)
                return Task.FromResult("Username not found");

            var key = _configuration.GetValue<string>("JwtConfig:Key");
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
            };

            var security_token = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(security_token);
            return Task.FromResult(token);
        }
    }
}
