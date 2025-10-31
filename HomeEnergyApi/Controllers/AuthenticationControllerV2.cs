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
    [ApiVersion("2.0")]
    [Route("v{version:apiVersion}/authentication")]
    public class AuthenticationControllerV2 : ControllerBase
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secret;
        private readonly IUserRepository userRepository;
        private readonly ValueHasher passwordHasher;
        private readonly ValueEncryptor valueEncryptor;
        private readonly IMapper mapper;

        public AuthenticationControllerV2(IConfiguration configuration,
                                        IUserRepository userRepository,
                                        ValueHasher passwordHasher,
                                        ValueEncryptor valueEncryptor,
                                        IMapper mapper)
        {
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
            _secret = configuration["Jwt:Secret"];
            this.userRepository = userRepository;
            this.passwordHasher = passwordHasher;
            this.valueEncryptor = valueEncryptor;
            this.mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDtoV2 userDto)
        {
            var existingUser = userRepository.FindByUsername(userDto.Username);
            if (existingUser != null)
            {
                return BadRequest("Username is already taken.");
            }

            var user = mapper.Map<User>(userDto);

            string hashPassword = passwordHasher.HashPassword(userDto.Password);
            user.HashedPassword = hashPassword;

            string encryptedAddress = valueEncryptor.Encrypt(BuildFullAddress(userDto));
            user.EncryptedAddress = encryptedAddress;


            userRepository.Save(user);
            if (userRepository.UserIsAdmin(userDto.Role))
                return Ok("Admin registered successfully.");
            else
                return Ok("User registered successfully.");
        }

        [HttpPost("token")]
        public IActionResult Token([FromBody] UserDtoV2 userDto)
        {
            var user = userRepository.FindByUsername(userDto.Username);
            if (user == null || !passwordHasher.VerifyPassword(user.HashedPassword, userDto.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            string address = valueEncryptor.Decrypt(user.EncryptedAddress);

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

        private string BuildFullAddress(UserDtoV2 userDto)
        {
            return $"{userDto.Address.StreetAddress} {userDto.Address.City}, {userDto.Address.ZipCode}";
        }
    }
}
