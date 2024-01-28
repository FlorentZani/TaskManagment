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
        //Get all user tasks 
        [HttpGet("GetAllUserTasks"),Authorize]
        public async Task<ActionResult<List<UserTaskGetDTO>>> GetAllTasksForUser()
        {
            try
            {
                var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "name");
                if (userNameClaim == null)
                {
                    return Unauthorized(new { message = "User not authorized." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userNameClaim.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var userTasks = await _context.Tasks.Where(task => task.UserId == user.UserId).ToListAsync();
                var userTaskGetDTOs = userTasks.Select(task => new UserTaskGetDTO
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    Description = task.Description,
                    Deadline = task.DeadLine,
                    Status = task.isFinishied ? "FINISHED" : "UNFINISHED"
                }).ToList();

                return userTaskGetDTOs;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        //Add task
        [HttpPost("AddTask"), Authorize]
        public async Task<ActionResult> AddTask(UserTaskDTO taskDto)
        {
            try
            {
                var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "name");
                if (userNameClaim == null)
                {
                    return Unauthorized(new { message = "User not authorized." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userNameClaim.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
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

                return Ok(new { message = "Task added successfully.", TaskId = userTask.TaskId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while adding the task.", exception = ex.Message });
            }
        }

        // Edit task
        [HttpPut("EditTask/{taskId}"), Authorize]
        public async Task<ActionResult> EditTask(Guid taskId, UserTaskDTO taskDto)
        {
            try
            {
                var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "name");
                if (userNameClaim == null)
                {
                    return Unauthorized(new { message = "User not authorized." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userNameClaim.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound(new { message = "Task not found." });
                }

                if (task.UserId != user.UserId)
                {
                    return BadRequest(new { message = "Access denied. You can only edit your own tasks." });
                }

                task.Title = taskDto.Title;
                task.Description = taskDto.Description;
                task.DeadLine = taskDto.DeadLine;

                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Task updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while updating the task.", exception = ex.Message });
            }
        }

        // Delete Task
        [HttpDelete("DeleteTask/{taskId}"), Authorize]
        public async Task<ActionResult> DeleteTask(Guid taskId)
        {
            try
            {
                var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "name");
                if (userNameClaim == null)
                {
                    return Unauthorized(new { message = "User not authorized." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userNameClaim.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound(new { message = "Task not found." });
                }

                if (task.UserId != user.UserId)
                {
                    return BadRequest(new { message = "Access denied. You can only delete your own tasks." });
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Task deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while deleting the task.", exception = ex.Message });
            }
        }

        // Change status
        [HttpPut("ChangeStatus/{taskId}"), Authorize]
        public async Task<ActionResult> ChangeStatus(UserTaskStatusDTO request, Guid taskId)
        {
            try
            {
                var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == "name");
                if (userNameClaim == null)
                {
                    return Unauthorized(new { message = "User not authorized." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userNameClaim.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound(new { message = "Task not found." });
                }

                if (task.UserId != user.UserId)
                {
                    return BadRequest(new { message = "Access denied. You can only change the status of your own tasks." });
                }

                task.isFinishied = request.isFinished;

                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Status updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while updating the status.", exception = ex.Message });
            }
        }

    }
}
