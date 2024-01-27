using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagmentSystem.DTOs;
using TaskManagmentSystem.Entities;

namespace TaskManagmentSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }

        //Get user data by ID 
        [HttpGet("UserDataByUserName/{userName}")]
        public async Task<ActionResult<UserDTO>> getUserByUserName(String userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound(new { message = "Username not found" });
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
        [HttpPut("EditUser/{userName}")]
        public async Task<ActionResult> EditUserInfo(String userName, UserRegisterDTO UpdatedUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound(new { message = "Username not found" });
            }

            byte[] PasswordHash;
            byte[] PasswordSalt;
            AuthController.CreatePasswordHash(UpdatedUser.Password, out PasswordHash, out PasswordSalt);

            user.UserName = UpdatedUser.UserName;
            user.Name = UpdatedUser.Name;
            user.LastName = UpdatedUser.LastName;
            user.PasswordHash = PasswordHash;
            user.PasswordSalt = PasswordSalt;

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
        [HttpDelete("DeleteUser/{userName}")]
        public async Task<ActionResult> DeleteUser(String userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
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
    }
}
