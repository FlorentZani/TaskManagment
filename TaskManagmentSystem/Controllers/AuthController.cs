using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagmentSystem.DTOs;
using TaskManagmentSystem.Entities;

namespace TaskManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Handle user Registration
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserRegisterDTO request)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
                {
                    return BadRequest(new { message = "Username is taken" });
                }

                CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

                User newUser = new User
                {
                    UserName = request.UserName,
                    Name = request.Name,
                    LastName = request.LastName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Registration successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while registering the user.", exception = ex.Message });
            }
        }

        // Handle User Login 
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(UserLoginDTO request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);

                if (user == null)
                {
                    return BadRequest(new { message = "User not found" });
                }

                if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return BadRequest(new { message = "Wrong Password" });
                }

                string token = CreateToken(user);
                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while logging in.", exception = ex.Message });
            }
        }

        // Handle password hashing
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // Handle password verification
        private bool VerifyPasswordHash(string inputedPassword, byte[] userPasswordHash, byte[] userPasswordSalt)
        {
            using (var hmac = new HMACSHA512(userPasswordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputedPassword));
                return computedHash.SequenceEqual(userPasswordHash);
            }
        }

        // Handle Token 
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("name", user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
