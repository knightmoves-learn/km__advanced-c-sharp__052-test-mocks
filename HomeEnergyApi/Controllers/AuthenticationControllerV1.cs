using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeEnergyApi.Dtos;
using HomeEnergyApi.Models;
using HomeEnergyApi.Security;


namespace HomeEnergyApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/authentication")]
    public class AuthenticationControllerV1 : ControllerBase
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secret;
        private readonly IUserRepository userRepository;
        private readonly ValueHasher passwordHasher;
        private readonly ValueEncryptor valueEncryptor;
        private readonly IMapper mapper;
        private readonly ILogger<AuthenticationControllerV1> logger;

        public AuthenticationControllerV1(IConfiguration configuration,
                                        IUserRepository userRepository,
                                        ValueHasher passwordHasher,
                                        ValueEncryptor valueEncryptor,
                                        IMapper mapper,
                                        ILogger<AuthenticationControllerV1> logger)
        {
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
            _secret = configuration["Jwt:Secret"];
            this.userRepository = userRepository;
            this.passwordHasher = passwordHasher;
            this.valueEncryptor = valueEncryptor;
            this.mapper = mapper;
            this.logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDtoV1 userDto)
        {   
            var existingUser = userRepository.FindByUsername(userDto.Username);
            if (existingUser != null)
            {
                return BadRequest("Username is already taken.");
            }

            var user = mapper.Map<User>(userDto);

            string hashPassword = passwordHasher.HashPassword(userDto.Password);
            logger.LogInformation("Hashed Password: " + hashPassword);
            user.HashedPassword = hashPassword;

            string encryptedStreetAddress = valueEncryptor.Encrypt(userDto.HomeStreetAddress);
            logger.LogInformation("Encrypted Street Address: " + encryptedStreetAddress);
            user.EncryptedAddress = encryptedStreetAddress;


            userRepository.Save(user);
            logger.LogDebug($"Saved Username: {user.Username}");
            return Ok("User registered successfully.");
        }

        [HttpPost("token")]
        public IActionResult Token([FromBody] UserDtoV1 userDto)
        {
            var user = userRepository.FindByUsername(userDto.Username);
            if (user == null || !passwordHasher.VerifyPassword(user.HashedPassword, userDto.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            string streetAddress = valueEncryptor.Decrypt(user.EncryptedAddress);

            string token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
