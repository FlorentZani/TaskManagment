using System.ComponentModel.DataAnnotations;

namespace TaskManagmentSystem.Entities
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName {  get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public List<UserTask> Tasks { get; set; }

    }
}
