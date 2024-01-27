using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagmentSystem.DTOs;
using TaskManagmentSystem.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

        //Handle user Registration
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserRegisterDTO request)
        {
            byte[] passwordHash;
            byte[] passwordSalt;

            if(await _context.Users.AnyAsync(u => u.UserName == request.UserName))
            {
                return BadRequest("Username is taken");
            }

            CreatePasswordHash(request.Password,out passwordHash,out passwordSalt);

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

            
            return Ok("Registration Success");


        }

        //Handle User Login 
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(UserLoginDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            if(!VerifyPasswordHash(request.Password,user.PasswordHash,user.PasswordSalt))
            {
                return BadRequest("Wrong Password");
            }

            string token = CreateToken(user);
            var response = new
            {
                token = token
            };
            return Ok(response);
        }


        //Handle password hashing
        public static void CreatePasswordHash(String password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
            

        }
        //Handle password verification
        private bool VerifyPasswordHash(string inputedPassword, byte[] userPasswordHash, byte[] userPasswordSalt)
        {
            using (var hmac = new HMACSHA512(userPasswordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputedPassword));
                return computedHash.SequenceEqual(userPasswordHash);
            }
        }

        //Handle Token 
        private string CreateToken(User user)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim("name", user.UserName));

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires:DateTime.Now.AddDays(1),
                signingCredentials:creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;



        }



    }
}
