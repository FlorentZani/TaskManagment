namespace TaskManagmentSystem.DTOs
{
    public class UserTaskGetDTO
    {
        public Guid TaskId { get; set; }
        public DateTime Deadline { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public String Status { get; set; }

    }
}
