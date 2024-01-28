using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagmentSystem.DTOs;
using TaskManagmentSystem.Entities;

namespace TaskManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }
        
        [HttpGet("UserDataByUserName"),Authorize]
        public async Task<ActionResult<UserDTO>> getUserByUserName()
        {
            var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userNameClaim == null)
            {
                return Unauthorized(new { message = "User not authorized." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userNameClaim.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            var response = new UserDTO
            {
                UserName = user.UserName,
                Name = user.Name,
                LastName = user.LastName,
            };

            return Ok(response);
        }

        //Edit User 
        [HttpPut("EditUser"),Authorize]
        public async Task<ActionResult> EditUserInfo([FromBody] EditUserDTO UpdatedUser)
        {
            var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userNameClaim == null)
            {
                return Unauthorized(new { message = "User not authorized." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userNameClaim.Value);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            if (await _context.Users.AnyAsync(u => u.UserName == UpdatedUser.UserName))
            {
                return BadRequest(new { message = "Username is taken" });
            }
            user.UserName = UpdatedUser.UserName;
            user.Name = UpdatedUser.Name;
            user.LastName = UpdatedUser.LastName;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating user in the database", exception = ex.Message });
            }
        }

        //Delete User 
        [HttpDelete("DeleteUser"),Authorize]
        public async Task<ActionResult> DeleteUser()
        {
            var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userNameClaim == null)
            {
                return Unauthorized(new { message = "User not authorized." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userNameClaim.Value);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            
            if (user == null)
            {
                return NotFound(new { message = "Username not found" });
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error removing user from the database", exception = ex.Message });
            }
        }

        //Edit User password
        [HttpPut("EditUserPassword"), Authorize]
        public async Task<ActionResult> EditUserInfo([FromBody] EditUserPasswordDTO UpdatedUser)
        {
            var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userNameClaim == null)
            {
                return Unauthorized(new { message = "User not authorized." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userNameClaim.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            byte[] OldPasswordHash = user.PasswordHash;
            byte[] OldPasswordSalt = user.PasswordSalt;

            if (AuthController.VerifyPasswordHash(UpdatedUser.OldPassword, OldPasswordHash, OldPasswordSalt) != true)
            {
                return BadRequest(new { message = "Old password is wrong" });
            }
            else
            {
                byte[] PasswordHash;
                byte[] PasswordSalt;
                AuthController.CreatePasswordHash(UpdatedUser.NewPassword, out PasswordHash, out PasswordSalt);

                user.PasswordHash = PasswordHash;
                user.PasswordSalt = PasswordSalt;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "User updated password successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error updating user password in the database", exception = ex.Message });
            }
        }



    }
}
