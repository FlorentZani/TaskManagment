using TaskManagmentSystem.Entities;

namespace TaskManagmentSystem.DTOs
{
    public class UserTaskDTO
    {
        public String Title { get; set; }
        public String Description { get; set; }
        public DateTime DeadLine { get; set; }
        
    }
}
