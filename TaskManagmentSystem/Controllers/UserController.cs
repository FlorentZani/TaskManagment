using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagmentSystem.DTOs;
using TaskManagmentSystem.Entities;

namespace TaskManagmentSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly DataContext _context;

        public UserController (DataContext context)
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
                var badResponse = new
                {
                    message = "Username not found"
                };
                return BadRequest(badResponse);
            }
            var okResponse = new UserDTO 
            { 
                UserName =  user.UserName,
                Name = user.Name,
                LastName = user.LastName,

            };

            return Ok(okResponse);
        }

        //Edit User 
        [HttpPut("EditUser/{userName}")]
        public async Task<ActionResult> EditUserInfo(String userName , UserRegisterDTO UpdatedUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>u.UserName == userName);
            if (user == null)
            {
                var badResponse = new
                {
                    message = "Username not found"
                };
                return BadRequest(badResponse);
            }
            byte[] PasswordHash;
            byte[] PasswordSalt;
            AuthController.CreatePasswordHash(UpdatedUser.Password,out PasswordHash , out PasswordSalt);

            user.UserName = UpdatedUser.UserName;
            user.Name = UpdatedUser.Name;
            user.LastName = UpdatedUser.LastName;
            user.PasswordHash = PasswordHash;
            user.PasswordSalt = PasswordSalt;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var BadResponse = new
                {
                    message = "User update was not saved in the database "
                };
                return BadRequest(BadResponse);
            }

            var okResponse = new
            {
                message = "Update gone succesfully"
            };
            return Ok(okResponse);
            
        }

        //Delete User 
        [HttpDelete("DeleteUser/{userName}")]
        public async Task<ActionResult> DeleteUser(String userName) 
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                var badResponse = new
                {
                    message = "Username not found"
                };
                return BadRequest(badResponse);
            }
            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var badResponse = new
                {
                    message = "User was not removed from the database"
                };
                return BadRequest(badResponse);
            }
            var okResponse = new
            {
                message = "Username not found"
            };
            
            return Ok(okResponse);

        }
    }
}
