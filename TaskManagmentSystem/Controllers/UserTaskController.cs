using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagmentSystem.DTOs;
using TaskManagmentSystem.Entities;

namespace TaskManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTaskController : Controller
    {
        private readonly DataContext _context;

        public UserTaskController(DataContext context)
        {
            _context = context;
        }

        //Add tasks to the database 

        [HttpPost("AddTask"), Authorize]
        public async Task<ActionResult> AddTask(UserTaskDTO taskDto)
        {
            try
            {
                String userName = "";
                var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "name");
                if (userNameClaim != null)
                {
                    userName = userNameClaim.Value;
                }
                if (String.IsNullOrEmpty(userName))
                {
                    return Unauthorized(userName);
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (user == null)
                {
                    return NotFound("User not found." + userName);
                }

                var userTask = new UserTask
                {
                    UserId = user.UserId,
                    Title = taskDto.Title,
                    Description = taskDto.Description,
                    DeadLine = taskDto.DeadLine,
                    isFinishied = false
                };

                _context.Tasks.Add(userTask);
                await _context.SaveChangesAsync();

                var response = new
                {
                    TaskId = userTask.TaskId
                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            
        }

        [HttpPut("EditTask/{taskId}"), Authorize]
        public async Task<ActionResult> EditTask(int taskId, UserTaskDTO taskDto)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            // Update task properties
            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.DeadLine = taskDto.DeadLine;

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            return Ok("Task updated successfully.");
        }

        // Delete Task endpoint
        [HttpDelete("DeleteTask/{taskId}"), Authorize]
        public async Task<ActionResult> DeleteTask(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok("Task deleted successfully.");
        }

    }
}
