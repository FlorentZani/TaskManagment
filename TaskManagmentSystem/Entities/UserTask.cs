using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TaskManagmentSystem.Entities
{
    public class UserTask
    {
        [Key]
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public DateTime DeadLine { get; set; }
        public bool isFinishied { get; set; }
        [JsonIgnore]
        public User User { get; set; }
    }
}
