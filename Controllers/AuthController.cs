using DocumentQA.Data;
using DocumentQA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DocumentQA.Controllers
{


    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly VectorDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(VectorDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ---------------------------
        // Models
        // ---------------------------
        public class RegisterRequest
        {
            public string Email { get; set; } = default!;
            public string Password { get; set; } = default!;
        }

        public record LoginRequest(string Email, string Password);

        // ---------------------------
        // POST: /api/auth/register
        // ---------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            return StatusCode(503, "Registration is temporarily disabled.");

            var exists = await _context.Users
                .AnyAsync(u => u.Email == request.Email);


            // Ensure email is unique and password meets the requirements
            if (exists)
            {
                return BadRequest("Email already registered.");
            }

            if (!IsStrongPassword(request.Password))
            {
                return BadRequest("Password must be at least 8 characters and include uppercase, lowercase and number.");
            }

            using var hmac = new HMACSHA512();

            var user = new UserEntity
            {
                Email = request.Email,
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password))
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User created.");
        }

        // ---------------------------
        // POST: /api/auth/login
        // ---------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Admin login via environment variables
            var adminUser = _config["ADMIN_USERNAME"];
            var adminPass = _config["ADMIN_PASSWORD"];

            if (request.Email == adminUser && request.Password == adminPass)
            {
                var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, "admin"), // stable virtual ID
            new Claim(JwtRegisteredClaimNames.Email, adminUser),
            new Claim(ClaimTypes.Role, "Admin")
        };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds
                );

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { token = jwt });
            }

            await Task.Delay(500); // sleep for 0.5 second

            // 2. Normal DB user login
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid credentials.");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));

            if (!computedHash.SequenceEqual(user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var userClaims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id), // DB user ID
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, "User")
    };
            

            var userKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var userCreds = new SigningCredentials(userKey, SecurityAlgorithms.HmacSha256);

            var userToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: userCreds
            );

            var userJwt = new JwtSecurityTokenHandler().WriteToken(userToken);

            return Ok(new { token = userJwt });
        }

        private bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return password.Length >= 8 &&
                   hasUpper &&
                   hasLower &&
                   hasDigit;
        }


    }
}