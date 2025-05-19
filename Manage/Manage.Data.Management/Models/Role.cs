using System.ComponentModel.DataAnnotations;

namespace Manage.Data.Management.Models
{
    public class Role
    {
        public Role()
        {

        }
        [Key]
        public int Id { get; set; }
        public required string Title { get; set; }
        public bool Delete { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RACC> RACCs { get; set; } = new List<RACC>();

    }
}
