using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manage.Data.Management.Models
{
    public class User
    {
        public User()
        {

        }
        [Key]
        public int Id { get; set; }
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }   // Hashed
        public string? ProfilePic { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string EmailValidation { get; set; } = "NO";
        [Phone]
        public string? Phonenumber { get; set; }
        public string PhonenumberValidation { get; set; } = "NO";
        public string Validation { get; set; } = "NO";
        public string RefrenceId { get; set; } = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
        public bool Delete { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;
        public DateTime LastLoginDate { get; set; } = DateTime.Now;
        public virtual User? Parent { get; set; }
        public virtual IEnumerable<UserRole> UserRoles { get; set; } = Enumerable.Empty<UserRole>();
        public virtual IEnumerable<UACC> UACCs { get; set; } = Enumerable.Empty<UACC>();

    }

}
